using System;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;
using System.Collections.Generic;

namespace ass01_FE.BusinessLogic.Services;

public class NewsViewService
{
    private readonly ass01_FE.Infrastructure.Services.OfflineNewsService _offlineNewsService;
    private readonly ass01_FE.Infrastructure.Services.OfflineCategoryService _offlineCategoryService;
    private readonly ass01_FE.Infrastructure.Services.OfflineTagService _offlineTagService;

    public NewsViewService(
        ass01_FE.Infrastructure.Services.OfflineNewsService offlineNewsService, 
        ass01_FE.Infrastructure.Services.OfflineCategoryService offlineCategoryService, 
        ass01_FE.Infrastructure.Services.OfflineTagService offlineTagService)
    {
        _offlineNewsService = offlineNewsService;
        _offlineCategoryService = offlineCategoryService;
        _offlineTagService = offlineTagService;
    }

    public async Task<(IEnumerable<object> Items, int Count, CategoryListResult? Categories, IEnumerable<object>? Tags, bool IsOffline)> GetStaffNewsDataAsync(
        string? keyword, short? categoryId, string? tagName, DateTime? startDate, DateTime? endDate, string? authorName, bool? newsStatus, int skip, int top)
    {
        var newsResult = await _offlineNewsService.GetStaffNewsWithOfflineFallbackAsync(keyword, categoryId, tagName, startDate, endDate, authorName, newsStatus, skip, top);
        var categoriesResult = await _offlineCategoryService.GetCategoriesWithOfflineFallbackAsync();
        var tagsResult = await _offlineTagService.GetTagsWithOfflineFallbackAsync();

        var categories = categoriesResult.Data;
        var isOffline = newsResult.IsOffline || categoriesResult.IsOffline || tagsResult.IsOffline;

        return (newsResult.Data.Items, newsResult.Data.TotalCount, categories, tagsResult.Data, isOffline);
    }
}
