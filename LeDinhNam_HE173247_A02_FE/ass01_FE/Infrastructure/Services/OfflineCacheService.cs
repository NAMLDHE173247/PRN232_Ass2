using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ass01_FE.Presentation.Models.News;
using ass01_FE.DataAccess.Services;
using ass01_FE.Presentation.Models.Dashboard;
using ass01_FE.Presentation.Models.Dashboard;

namespace ass01_FE.Infrastructure.Services;

public interface IOfflineCacheService
{
    Task SaveNewsArticlesAsync(List<NewsArticleDto> articles);
    Task<(List<NewsArticleDto>? Data, DateTimeOffset? CachedAt)> GetNewsArticlesAsync();

    Task SaveCategoriesAsync(List<CategoryDto> categories);
    Task<(List<CategoryDto>? Data, DateTimeOffset? CachedAt)> GetCategoriesAsync();

    Task SaveTagsAsync(List<TagDto> tags);
    Task<(List<TagDto>? Data, DateTimeOffset? CachedAt)> GetTagsAsync();

    Task SaveDashboardStatisticsAsync(DashboardViewModel dashboard);
    Task<(DashboardViewModel? Data, DateTimeOffset? CachedAt)> GetDashboardStatisticsAsync();
}

public class OfflineCacheService : IOfflineCacheService
{
    private readonly IDistributedCache _cache;
    private readonly int _cacheDurationHours;

    private const string NewsArticlesKey = "FUNews_NewsArticles";
    private const string CategoriesKey = "FUNews_Categories";
    private const string TagsKey = "FUNews_Tags";
    private const string DashboardKey = "FUNews_AdminDashboard";

    public OfflineCacheService(IDistributedCache cache, IConfiguration config)
    {
        _cache = cache;
        _cacheDurationHours = config.GetValue<int>("OfflineCache:CacheDurationHours", 12);
    }

    private DistributedCacheEntryOptions GetOptions()
    {
        return new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(_cacheDurationHours)
        };
    }

    private class CacheWrapper<T>
    {
        public T? Data { get; set; }
        public DateTimeOffset CachedAt { get; set; }
    }

    private async Task SaveAsync<T>(string key, T data)
    {
        var wrapper = new CacheWrapper<T>
        {
            Data = data,
            CachedAt = DateTimeOffset.Now
        };
        var json = JsonSerializer.Serialize(wrapper);
        await _cache.SetStringAsync(key, json, GetOptions());
    }

    private async Task<(T? Data, DateTimeOffset? CachedAt)> GetAsync<T>(string key)
    {
        var json = await _cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(json))
        {
            return (default, null);
        }

        try
        {
            var wrapper = JsonSerializer.Deserialize<CacheWrapper<T>>(json);
            if (wrapper != null)
            {
                return (wrapper.Data, wrapper.CachedAt);
            }
        }
        catch
        {
            // Ignore parse errors, just return null
        }
        
        return (default, null);
    }

    public Task SaveNewsArticlesAsync(List<NewsArticleDto> articles) => SaveAsync(NewsArticlesKey, articles);
    public Task<(List<NewsArticleDto>? Data, DateTimeOffset? CachedAt)> GetNewsArticlesAsync() => GetAsync<List<NewsArticleDto>>(NewsArticlesKey);

    public Task SaveCategoriesAsync(List<CategoryDto> categories) => SaveAsync(CategoriesKey, categories);
    public Task<(List<CategoryDto>? Data, DateTimeOffset? CachedAt)> GetCategoriesAsync() => GetAsync<List<CategoryDto>>(CategoriesKey);

    public Task SaveTagsAsync(List<TagDto> tags) => SaveAsync(TagsKey, tags);
    public Task<(List<TagDto>? Data, DateTimeOffset? CachedAt)> GetTagsAsync() => GetAsync<List<TagDto>>(TagsKey);

    public Task SaveDashboardStatisticsAsync(DashboardViewModel dashboard) => SaveAsync(DashboardKey, dashboard);
    public Task<(DashboardViewModel? Data, DateTimeOffset? CachedAt)> GetDashboardStatisticsAsync() => GetAsync<DashboardViewModel>(DashboardKey);
}
