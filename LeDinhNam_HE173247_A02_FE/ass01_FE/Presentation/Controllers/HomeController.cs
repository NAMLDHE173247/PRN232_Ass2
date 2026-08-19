using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;
using ass01_FE.Presentation.Models.News;
using System.Dynamic;

namespace ass01_FE.Presentation.Controllers;

public class HomeController : Controller
{
    private readonly ass01_FE.Infrastructure.Services.OfflineNewsService _offlineNewsService;
    private readonly ass01_FE.Infrastructure.Clients.AnalyticsApiClient _analyticsApiClient;
    private readonly NewsApiService _newsApiService;

    public HomeController(ass01_FE.Infrastructure.Services.OfflineNewsService offlineNewsService, ass01_FE.Infrastructure.Clients.AnalyticsApiClient analyticsApiClient, NewsApiService newsApiService)
    {
        _offlineNewsService = offlineNewsService;
        _analyticsApiClient = analyticsApiClient;
        _newsApiService = newsApiService;
    }

    public async Task<IActionResult> Index(string? keyword, int page = 1)
    {
        ViewBag.Keyword = keyword;
        int pageSize = 100;
        if (page < 1) page = 1;

        var result = await _offlineNewsService.GetActiveNewsWithOfflineFallbackAsync(keyword, (page - 1) * pageSize, pageSize);
        var totalPages = (int)System.Math.Ceiling(result.Data.TotalCount / (double)pageSize);
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.IsOffline = result.IsOffline;
        return View(result.Data.Items);
    }

    public async Task<IActionResult> Detail(string id)
    {
        // 1. Increment View Count
        await _newsApiService.IncrementNewsViewCountAsync(id);

        // 2. Fetch Article
        var article = await _newsApiService.GetNewsByIdAsync(id);
        if (article == null)
        {
            return NotFound();
        }

        // 3. Fetch Recommended Articles (max 3)
        var related = await _analyticsApiClient.GetRecommendedArticlesAsync(id);
        var topRelated = related?.Take(3).ToList() ?? new List<ass01_FE.Presentation.Models.News.NewsArticleDto>();

        dynamic model = new ExpandoObject();
        model.Article = article;
        model.Related = topRelated;

        return View(model);
    }
}

