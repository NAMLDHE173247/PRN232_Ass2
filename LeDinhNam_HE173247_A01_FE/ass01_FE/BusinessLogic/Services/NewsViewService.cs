using System;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;
using System.Collections.Generic;

namespace ass01_FE.BusinessLogic.Services;

public class NewsViewService
{
    private readonly NewsApiService _newsApiService;
    private readonly CategoryApiService _categoryApiService;
    private readonly TagApiService _tagApiService;

    public NewsViewService(NewsApiService newsApiService, CategoryApiService categoryApiService, TagApiService tagApiService)
    {
        _newsApiService = newsApiService;
        _categoryApiService = categoryApiService;
        _tagApiService = tagApiService;
    }

    public async Task<(IEnumerable<object> Items, int Count, CategoryListResult? Categories, IEnumerable<object> Tags)> GetStaffNewsDataAsync(
        string token, string? keyword, short? categoryId, string? tagName, DateTime? startDate, DateTime? endDate, string? authorName, bool? newsStatus, int skip, int top)
    {
        var (items, count) = await _newsApiService.GetStaffNewsAsync(token, keyword, categoryId, tagName, startDate, endDate, authorName, newsStatus, skip, top);
        var categories = await _categoryApiService.GetCategoriesAsync(token, null, 0, 100);
        var tags = await _tagApiService.GetTagsAsync(token);

        return (items, count, categories, tags);
    }
}
