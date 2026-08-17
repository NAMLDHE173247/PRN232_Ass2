using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using ass01_FE.DataAccess.Services;
using ass01_FE.Presentation.Models.Report;

namespace ass01_FE.Presentation.Controllers;

public class ReportController : Controller
{
    private readonly ReportApiService _reportApiService;

    public ReportController(ReportApiService reportApiService)
    {
        _reportApiService = reportApiService;
    }

    private bool IsAdmin()
    {
        return HttpContext.Session.GetString("UserRole") == "Admin";
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Index", "Home");
        }

        var model = new ReportViewModel();

        if (startDate.HasValue && endDate.HasValue)
        {
            model.Request.StartDate = startDate;
            model.Request.EndDate = endDate;

            if (startDate > endDate)
            {
                model.ErrorMessage = "Start date must be earlier than or equal to end date.";
                return View(model);
            }

            var token = HttpContext.Session.GetString("AccessToken") ?? string.Empty;
            var stats = await _reportApiService.GetReportAsync(token, startDate.Value, endDate.Value);
            
            if (stats == null)
            {
                model.ErrorMessage = "Failed to fetch report from server.";
            }
            else
            {
                model.Statistics = stats;
            }
        }
        else
        {
            // Default to past 30 days if no dates provided
            model.Request.EndDate = DateTime.Today;
            model.Request.StartDate = DateTime.Today.AddDays(-30);
        }

        return View(model);
    }
}
