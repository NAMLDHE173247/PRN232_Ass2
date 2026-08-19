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
        var cacheService = scope.ServiceProvider.GetRequiredService<IOfflineCacheService>();
        var newsApiService = scope.ServiceProvider.GetRequiredService<NewsApiService>();
        var categoryApiService = scope.ServiceProvider.GetRequiredService<CategoryApiService>();
        var tagApiService = scope.ServiceProvider.GetRequiredService<TagApiService>();

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
    }
}
