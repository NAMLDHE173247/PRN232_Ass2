using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;
using ass01_FE.Infrastructure.Helpers;
using ass01_FE.Infrastructure.Models;
using ass01_FE.DataAccess.Services;

namespace ass01_FE.Infrastructure.Services;

public class OfflineCategoryService
{
    private readonly CategoryApiService _apiService;
    private readonly IOfflineCacheService _cacheService;

    public OfflineCategoryService(CategoryApiService apiService, IOfflineCacheService cacheService)
    {
        _apiService = apiService;
        _cacheService = cacheService;
    }

    public async Task<ApiDataResult<CategoryListResult>> GetCategoriesWithOfflineFallbackAsync(string? searchKeyword = null, int skip = 0, int top = 100)
    {
        try
        {
            var categoriesResult = await _apiService.GetCategoriesAsync(searchKeyword, skip, top);
            if (skip == 0 && string.IsNullOrEmpty(searchKeyword))
            {
                await _cacheService.SaveCategoriesAsync(categoriesResult?.Value ?? new List<CategoryDto>());
            }
            return ApiDataResult<CategoryListResult>.Online(categoriesResult ?? new CategoryListResult());
        }
        catch (HttpRequestException ex) when (OfflineHelper.IsApiUnavailableResponse(ex))
        {
            var cached = await _cacheService.GetCategoriesAsync();
            var result = new CategoryListResult 
            { 
                Value = cached.Data ?? new List<CategoryDto>(), 
                Count = cached.Data?.Count ?? 0 
            };
            return ApiDataResult<CategoryListResult>.Offline(result, cached.CachedAt);
        }
    }
}
