using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ass01_FE.Infrastructure.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ass01_FE.Infrastructure.Handlers;

public class AuthenticatedHttpClientHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TokenRefreshCoordinator _coordinator;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuthenticatedHttpClientHandler> _logger;

    public AuthenticatedHttpClientHandler(
        IHttpContextAccessor httpContextAccessor,
        TokenRefreshCoordinator coordinator,
        IServiceProvider serviceProvider,
        ILogger<AuthenticatedHttpClientHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _coordinator = coordinator;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            // Background worker or no session -> proceed without token
            return await base.SendAsync(request, cancellationToken);
        }

        var sessionId = context.Session.Id;

        // Try to check if token needs proactive refresh
        await EnsureTokenFreshAsync(context, sessionId);

        // Fetch latest token state
        var tokenState = _coordinator.GetState(sessionId);
        var token = tokenState?.AccessToken ?? context.Session.GetString("AccessToken");

        // We clone the request BEFORE the first send, so if it fails with 401 we can retry without payload consumption issues.
        // But only if we actually attached a token (if we didn't, a 401 is expected and no refresh will help).
        HttpRequestMessage? clonedRequest = null;
        if (!string.IsNullOrEmpty(token))
        {
            clonedRequest = await CloneHttpRequestMessageAsync(request);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Received 401 Unauthorized. Attempting immediate token refresh for session {SessionId}", sessionId);
            var refreshSuccess = await TryRefreshTokenAsync(context, sessionId);
            if (refreshSuccess && clonedRequest != null)
            {
                var newState = _coordinator.GetState(sessionId);
                var newToken = newState?.AccessToken ?? context.Session.GetString("AccessToken");
                clonedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                
                // Retry sending the cloned request exactly once
                response.Dispose(); // dispose the 401 response
                response = await base.SendAsync(clonedRequest, cancellationToken);
            }
            else
            {
                clonedRequest?.Dispose();
            }
        }
        else
        {
            clonedRequest?.Dispose();
        }

        return response;
    }

    private async Task EnsureTokenFreshAsync(HttpContext context, string sessionId)
    {
        var expiresAtStr = context.Session.GetString("ExpiresAt");
        var refreshToken = context.Session.GetString("RefreshToken");
        var accessToken = context.Session.GetString("AccessToken");

        if (string.IsNullOrEmpty(refreshToken)) return; // No refresh token -> nothing to do

        DateTimeOffset expiresAt = DateTimeOffset.MinValue;
        if (!string.IsNullOrEmpty(expiresAtStr) && DateTimeOffset.TryParse(expiresAtStr, out var parsed))
        {
            expiresAt = parsed;
        }

        // If no access token but have refresh, or expires in < 5 mins
        if (string.IsNullOrEmpty(accessToken) || expiresAt <= DateTimeOffset.UtcNow.AddMinutes(5))
        {
            _logger.LogInformation("Proactive token refresh triggered for session {SessionId}", sessionId);
            await TryRefreshTokenAsync(context, sessionId);
        }
    }

    private async Task<bool> TryRefreshTokenAsync(HttpContext context, string sessionId)
    {
        var lockObj = _coordinator.GetLockForSession(sessionId);
        await lockObj.WaitAsync();

        try
        {
            // Re-read session to check if another thread already refreshed
            var currentExpiresAtStr = context.Session.GetString("ExpiresAt");
            if (!string.IsNullOrEmpty(currentExpiresAtStr) && DateTimeOffset.TryParse(currentExpiresAtStr, out var currentExpiresAt))
            {
                if (currentExpiresAt > DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    // Someone else refreshed it while we waited
                    _logger.LogInformation("Token was already refreshed by another thread for session {SessionId}", sessionId);
                    return true;
                }
            }

            var refreshToken = context.Session.GetString("RefreshToken");
            if (string.IsNullOrEmpty(refreshToken)) return false;

            // Use scoped service provider to get AuthRefreshClient, avoiding infinite loop
            using var scope = _serviceProvider.CreateScope();
            var refreshClient = scope.ServiceProvider.GetRequiredService<AuthRefreshClient>();

            var refreshPayload = new { refreshToken = refreshToken };
            var response = await refreshClient.Client.PostAsJsonAsync("api/auth/refresh", refreshPayload);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    context.Session.SetString("AccessToken", result.Token);
                    if (!string.IsNullOrEmpty(result.RefreshToken))
                        context.Session.SetString("RefreshToken", result.RefreshToken);
                    
                    DateTimeOffset newExpiresAt = DateTimeOffset.UtcNow.AddMinutes(60); // fallback
                    if (result.ExpiresAt.HasValue)
                        newExpiresAt = result.ExpiresAt.Value;

                    context.Session.SetString("ExpiresAt", newExpiresAt.ToString("o"));
                    
                    _coordinator.UpdateState(sessionId, result.Token, result.RefreshToken ?? refreshToken, newExpiresAt);
                    _logger.LogInformation("Token refreshed successfully for session {SessionId}", sessionId);
                    return true;
                }
            }

            // Refresh failed, token might be revoked
            _logger.LogWarning("Token refresh failed. Clearing session {SessionId}", sessionId);
            context.Session.Remove("AccessToken");
            context.Session.Remove("RefreshToken");
            context.Session.Remove("ExpiresAt");
            context.Session.Remove("UserRole");
            context.Session.Remove("AccountId");
            context.Session.Remove("Email");
            _coordinator.ClearState(sessionId);
            return false;
        }
        finally
        {
            lockObj.Release();
        }
    }

    private async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
    {
        var clone = new HttpRequestMessage(req.Method, req.RequestUri);

        // Copy content
        if (req.Content != null)
        {
            var bytes = await req.Content.ReadAsByteArrayAsync();
            var stream = new MemoryStream(bytes);
            clone.Content = new StreamContent(stream);
            if (req.Content.Headers != null)
            {
                foreach (var h in req.Content.Headers)
                    clone.Content.Headers.Add(h.Key, h.Value);
            }
        }

        clone.Version = req.Version;

        // Copy headers
        foreach (var header in req.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }
}

public class RefreshTokenResponse
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}
