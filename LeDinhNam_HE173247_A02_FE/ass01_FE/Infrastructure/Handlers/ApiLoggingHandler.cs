using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ass01_FE.Infrastructure.Handlers;

public class ApiLoggingHandler : DelegatingHandler
{
    private readonly ILogger<ApiLoggingHandler> _logger;

    public ApiLoggingHandler(ILogger<ApiLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var method = request.Method;
        var uri = request.RequestUri;

        var sw = Stopwatch.StartNew();
        _logger.LogInformation("HTTP Request: {Method} {Uri}", method, uri);

        var response = await base.SendAsync(request, cancellationToken);
        
        sw.Stop();
        _logger.LogInformation("HTTP Response: {Method} {Uri} responded {StatusCode} in {ElapsedMs}ms", 
            method, uri, (int)response.StatusCode, sw.ElapsedMilliseconds);

        return response;
    }
}
