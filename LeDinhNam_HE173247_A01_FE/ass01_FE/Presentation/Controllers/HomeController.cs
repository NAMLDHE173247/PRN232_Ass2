using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;
using ass01_FE.Presentation.Models.News;
using System.Dynamic;

namespace ass01_FE.Presentation.Controllers;

public class HomeController : Controller
{
    private readonly NewsApiService _newsApiService;

    public HomeController(NewsApiService newsApiService)
    {
        _newsApiService = newsApiService;
    }

    public async Task<IActionResult> Index(string? keyword, int page = 1)
    {
        ViewBag.Keyword = keyword;
        int pageSize = 100;
        if (page < 1) page = 1;

        int skip = (page - 1) * pageSize;

        // Fetch paginated active news from backend
        var (items, totalCount) = await _newsApiService.GetActiveNewsAsync(keyword, skip, pageSize);

        var totalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize);
        
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(items);
    }

    public async Task<IActionResult> Detail(string id)
    {
        var article = await _newsApiService.GetNewsByIdAsync(id);
        if (article == null)
        {
            return NotFound();
        }

        var related = await _newsApiService.GetRelatedNewsAsync(id);

        dynamic model = new ExpandoObject();
        model.Article = article;
        model.Related = related;

        return View(model);
    }
}
