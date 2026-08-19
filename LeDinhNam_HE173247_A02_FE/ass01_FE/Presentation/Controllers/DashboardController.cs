using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ass01_FE.Infrastructure.Clients;
using ass01_FE.Presentation.Models.Dashboard;
using ass01_FE.Infrastructure.Filters;
using ass01_FE.DataAccess.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace ass01_FE.Presentation.Controllers;

[RoleAuthorize("Admin")]
public class DashboardController : Controller
{
    private readonly ass01_FE.Infrastructure.Services.OfflineDashboardService _offlineDashboardService;
    private readonly ass01_FE.Infrastructure.Services.OfflineCategoryService _offlineCategoryService;
    private readonly AnalyticsApiClient _analyticsApiClient;

    public DashboardController(ass01_FE.Infrastructure.Services.OfflineDashboardService offlineDashboardService, ass01_FE.Infrastructure.Services.OfflineCategoryService offlineCategoryService, AnalyticsApiClient analyticsApiClient)
    {
        _offlineDashboardService = offlineDashboardService;
        _offlineCategoryService = offlineCategoryService;
        _analyticsApiClient = analyticsApiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DashboardFilterViewModel filter)
    {
        if (filter.StartDate.HasValue && filter.EndDate.HasValue && filter.StartDate > filter.EndDate)
        {
            ViewBag.ErrorMessage = "Start date cannot be later than end date.";
            filter.StartDate = null; // Clear to allow rendering without broken state
        }

        var viewModel = new DashboardViewModel
        {
            CurrentFilter = filter
        };

        var categoriesResult = await _offlineCategoryService.GetCategoriesWithOfflineFallbackAsync();
        if (categoriesResult.Data != null && categoriesResult.Data.Value != null)
        {
            ViewBag.Categories = categoriesResult.Data.Value.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            }).ToList();
        }

        if (ViewBag.ErrorMessage == null)
        {
            var result = await _offlineDashboardService.GetDashboardWithOfflineFallbackAsync(filter);
            viewModel = result.Data ?? viewModel;
            ViewBag.IsOffline = result.IsOffline;
            if (result.IsOffline)
            {
                ViewBag.ErrorMessage = result.ErrorMessage;
            }
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ExportAnalyticsReportAsync(DashboardFilterViewModel filter)
    {
        try
        {
            var fileBytes = await _analyticsApiClient.ExportAnalyticsReportAsync(filter);
            var fileName = $"Analytics_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to export report. Analytics API might be unavailable.";
            return RedirectToAction(nameof(Index), filter);
        }
    }
}
