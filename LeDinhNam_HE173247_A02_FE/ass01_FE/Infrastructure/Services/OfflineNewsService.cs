using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;
using ass01_FE.Infrastructure.Helpers;
using ass01_FE.Infrastructure.Models;
using ass01_FE.Presentation.Models.News;

namespace ass01_FE.Infrastructure.Services;

public class OfflineNewsService
{
    private readonly NewsApiService _apiService;
    private readonly IOfflineCacheService _cacheService;

    public OfflineNewsService(NewsApiService apiService, IOfflineCacheService cacheService)
    {
        _apiService = apiService;
        _cacheService = cacheService;
    }

    public async Task<ApiDataResult<(List<NewsArticleDto> Items, int TotalCount)>> GetActiveNewsWithOfflineFallbackAsync(string? keyword = null, int skip = 0, int top = 5)
    {
        try
        {
            var result = await _apiService.GetActiveNewsAsync(keyword, skip, top);
            if (skip == 0 && string.IsNullOrEmpty(keyword))
            {
                await _cacheService.SaveNewsArticlesAsync(result.Items);
            }
            return ApiDataResult<(List<NewsArticleDto> Items, int TotalCount)>.Online(result);
        }
        catch (HttpRequestException ex) when (OfflineHelper.IsApiUnavailableResponse(ex))
        {
            var cached = await _cacheService.GetNewsArticlesAsync();
            return ApiDataResult<(List<NewsArticleDto> Items, int TotalCount)>.Offline(
                (cached.Data ?? new List<NewsArticleDto>(), cached.Data?.Count ?? 0), 
                cached.CachedAt);
        }
    }

    public async Task<ApiDataResult<(List<NewsArticleDto> Items, int TotalCount)>> GetStaffNewsWithOfflineFallbackAsync(
        string? keyword = null, short? categoryId = null, string? tagName = null, 
        System.DateTime? startDate = null, System.DateTime? endDate = null, 
        string? authorName = null, bool? newsStatus = null, int skip = 0, int top = 10)
    {
        try
        {
            var result = await _apiService.GetStaffNewsAsync(keyword, categoryId, tagName, startDate, endDate, authorName, newsStatus, skip, top);
            if (skip == 0 && string.IsNullOrEmpty(keyword) && categoryId == null && tagName == null && startDate == null && endDate == null && authorName == null && newsStatus == null)
            {
                await _cacheService.SaveNewsArticlesAsync(result.Items);
            }
            return ApiDataResult<(List<NewsArticleDto> Items, int TotalCount)>.Online(result);
        }
        catch (HttpRequestException ex) when (OfflineHelper.IsApiUnavailableResponse(ex))
        {
            var cached = await _cacheService.GetNewsArticlesAsync();
            return ApiDataResult<(List<NewsArticleDto> Items, int TotalCount)>.Offline(
                (cached.Data ?? new List<NewsArticleDto>(), cached.Data?.Count ?? 0), 
                cached.CachedAt);
        }
    }
}
