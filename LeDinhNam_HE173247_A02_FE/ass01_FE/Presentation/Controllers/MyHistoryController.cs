using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;

namespace ass01_FE.Presentation.Controllers;

public class MyHistoryController : Controller
{
    private readonly NewsApiService _newsApiService;

    public MyHistoryController(NewsApiService newsApiService)
    {
        _newsApiService = newsApiService;
    }

    private bool IsStaff()
    {
        return HttpContext.Session.GetString("UserRole") == "Staff";
    }

    

    public async Task<IActionResult> Index()
    {
        if (!IsStaff())
        {
            return RedirectToAction("Index", "Home");
        }

        var articles = await _newsApiService.GetMyHistoryAsync();
        
        return View(articles);
    }
}

