using System.Net.Http;

namespace ass01_FE.Infrastructure.Helpers;

public static class OfflineHelper
{
    public static bool IsApiUnavailableResponse(HttpRequestException ex)
    {
        if (ex.StatusCode.HasValue)
        {
            var code = (int)ex.StatusCode.Value;
            // 502 Bad Gateway, 503 Service Unavailable, 504 Gateway Timeout
            if (code == 502 || code == 503 || code == 504)
            {
                return true;
            }
        }
        
        // Network failures, connection refused, timeouts usually don't have a status code in HttpRequestException
        // Or they are inner exceptions
        if (ex.InnerException is System.Net.Sockets.SocketException)
        {
            return true;
        }

        // Broad fallback: if it's an HttpRequestException and not a 4xx error, treat as unavailable.
        // Actually, existing code might throw HttpRequestException for any EnsureSuccessStatusCode.
        // If StatusCode is 400, 401, 403, 404, 409 it's a business/auth error, NOT offline.
        if (ex.StatusCode.HasValue)
        {
            var code = (int)ex.StatusCode.Value;
            if (code >= 400 && code < 500)
            {
                return false;
            }
        }

        return true;
    }

    public static void ThrowIfOffline(HttpResponseMessage response)
    {
        var code = (int)response.StatusCode;
        if (code >= 500)
        {
            response.EnsureSuccessStatusCode();
        }
    }
}
