using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ass01_FE.Infrastructure;

public class TokenState
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}

public class TokenRefreshCoordinator
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _sessionLocks = new();
    private readonly ConcurrentDictionary<string, TokenState> _sessionStates = new();

    public SemaphoreSlim GetLockForSession(string sessionId)
    {
        return _sessionLocks.GetOrAdd(sessionId, _ => new SemaphoreSlim(1, 1));
    }

    public TokenState? GetState(string sessionId)
    {
        _sessionStates.TryGetValue(sessionId, out var state);
        return state;
    }

    public void UpdateState(string sessionId, string accessToken, string refreshToken, DateTimeOffset expiresAt)
    {
        _sessionStates[sessionId] = new TokenState
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt
        };
    }

    public void ClearState(string sessionId)
    {
        _sessionStates.TryRemove(sessionId, out _);
    }
}
