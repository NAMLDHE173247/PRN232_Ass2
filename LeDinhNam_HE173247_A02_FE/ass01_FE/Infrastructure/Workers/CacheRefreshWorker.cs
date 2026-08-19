using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ass01_FE.DataAccess.Services;
using ass01_FE.Infrastructure.Services;
using System.Collections.Generic;

namespace ass01_FE.Infrastructure.Workers;

public class CacheRefreshWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheRefreshWorker> _logger;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromHours(6);

    public CacheRefreshWorker(IServiceProvider serviceProvider, ILogger<CacheRefreshWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CacheRefreshWorker starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshCacheAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while refreshing offline cache.");
            }

            await Task.Delay(_refreshInterval, stoppingToken);
        }
    }

    private async Task RefreshCacheAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        var config = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var authApiService = scope.ServiceProvider.GetRequiredService<ass01_FE.DataAccess.Services.AuthApiService>();
        var workerTokenService = scope.ServiceProvider.GetRequiredService<ass01_FE.Infrastructure.Services.WorkerTokenService>();
        
        // 0. Authenticate Worker
        var email = config["AdminAccount:Email"];
        var password = config["AdminAccount:Password"];
        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            try
            {
                var loginResponse = await authApiService.LoginAsync(new ass01_FE.Presentation.Models.Auth.LoginViewModel { Email = email, Password = password });
                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    workerTokenService.AccessToken = loginResponse.Token;
                    _logger.LogInformation("Worker successfully authenticated.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Worker failed to authenticate. API calls might fail if they require auth.");
            }
        }

        var cacheService = scope.ServiceProvider.GetRequiredService<IOfflineCacheService>();
        var newsApiService = scope.ServiceProvider.GetRequiredService<NewsApiService>();
        var categoryApiService = scope.ServiceProvider.GetRequiredService<CategoryApiService>();
        var tagApiService = scope.ServiceProvider.GetRequiredService<TagApiService>();
        var analyticsApiClient = scope.ServiceProvider.GetRequiredService<ass01_FE.Infrastructure.Clients.AnalyticsApiClient>();

        // 1. Refresh Active News
        try
        {
            var (items, count) = await newsApiService.GetActiveNewsAsync(null, 0, 100);
            if (items != null)
            {
                await cacheService.SaveNewsArticlesAsync(items);
                _logger.LogInformation("Successfully refreshed active news cache. (Count: {Count})", items.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh active news cache in worker.");
        }

        // 2. Refresh Categories
        try
        {
            var catResult = await categoryApiService.GetCategoriesAsync(null, 0, 100);
            if (catResult != null && catResult.Value != null)
            {
                await cacheService.SaveCategoriesAsync(catResult.Value);
                _logger.LogInformation("Successfully refreshed categories cache. (Count: {Count})", catResult.Value.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh categories cache in worker.");
        }

        // 3. Refresh Tags
        try
        {
            var tags = await tagApiService.GetTagsAsync();
            if (tags != null)
            {
                await cacheService.SaveTagsAsync(tags);
                _logger.LogInformation("Successfully refreshed tags cache. (Count: {Count})", tags.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh tags cache in worker.");
        }

        // 4. Refresh Dashboard
        try
        {
            // Dashboard data from Analytics API
            var dashboardData = await analyticsApiClient.GetDashboardStatisticsAsync(new ass01_FE.Presentation.Models.Dashboard.DashboardFilterViewModel());
            
            // Map DashboardStatisticsDto to DashboardViewModel for Offline cache
            var viewModel = new ass01_FE.Presentation.Models.Dashboard.DashboardViewModel
            {
                Statistics = dashboardData,
                TrendingArticles = await analyticsApiClient.GetTrendingArticlesAsync(new ass01_FE.Presentation.Models.Dashboard.DashboardFilterViewModel())
            };
            
            if (viewModel != null)
            {
                await cacheService.SaveDashboardStatisticsAsync(viewModel);
                _logger.LogInformation("Successfully refreshed dashboard cache.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh dashboard cache in worker.");
        }
    }
}
