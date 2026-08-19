using ClosedXML.Excel;
using FUNewsManagement_AnalyticsAPI.Data;
using FUNewsManagement_AnalyticsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;

namespace FUNewsManagement_AnalyticsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly AnalyticsDbContext _context;

        public AnalyticsController(AnalyticsDbContext context)
        {
            _context = context;
        }

        private IQueryable<NewsArticle> ApplyFilter(IQueryable<NewsArticle> query, AnalyticsFilterDto filter)
        {
            if (filter.StartDate.HasValue)
            {
                query = query.Where(n => n.CreatedDate >= filter.StartDate.Value);
            }
            if (filter.EndDate.HasValue)
            {
                query = query.Where(n => n.CreatedDate <= filter.EndDate.Value);
            }
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(n => n.CategoryId == filter.CategoryId.Value);
            }
            if (filter.Status.HasValue)
            {
                query = query.Where(n => n.NewsStatus == filter.Status.Value);
            }
            if (filter.AuthorId.HasValue)
            {
                query = query.Where(n => n.CreatedById == filter.AuthorId.Value);
            }
            return query;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] AnalyticsFilterDto filter)
        {
            if (filter.StartDate.HasValue && filter.EndDate.HasValue && filter.StartDate > filter.EndDate)
            {
                return BadRequest("StartDate cannot be greater than EndDate");
            }

            var newsQuery = _context.NewsArticles.AsQueryable();
            newsQuery = ApplyFilter(newsQuery, filter);

            var totalNews = await newsQuery.CountAsync();
            var totalViews = await newsQuery.SumAsync(n => (int?)n.ViewCount) ?? 0;
            var totalAccounts = await _context.SystemAccounts.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();

            return Ok(new
            {
                TotalNews = totalNews,
                TotalViews = totalViews,
                TotalAccounts = totalAccounts,
                TotalCategories = totalCategories
            });
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetTrending()
        {
            var trendingNews = await _context.NewsArticles
                .Where(n => n.NewsStatus == true)
                .OrderByDescending(n => n.ViewCount)
                .ThenByDescending(n => n.CreatedDate)
                .Take(5)
                .Select(n => new
                {
                    n.NewsArticleId,
                    n.NewsTitle,
                    Headline = n.Headline,
                    n.ViewCount,
                    CategoryName = n.Category != null ? n.Category.CategoryName : null
                })
                .ToListAsync();

            return Ok(trendingNews);
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] AnalyticsFilterDto filter)
        {
            if (filter.StartDate.HasValue && filter.EndDate.HasValue && filter.StartDate > filter.EndDate)
            {
                return BadRequest("StartDate cannot be greater than EndDate");
            }

            var newsQuery = _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .AsQueryable();

            newsQuery = ApplyFilter(newsQuery, filter);

            var news = await newsQuery.OrderByDescending(n => n.CreatedDate).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("News Report");

            // Header
            worksheet.Cell(1, 1).Value = "News ID";
            worksheet.Cell(1, 2).Value = "Title";
            worksheet.Cell(1, 3).Value = "Category";
            worksheet.Cell(1, 4).Value = "Author";
            worksheet.Cell(1, 5).Value = "Views";
            worksheet.Cell(1, 6).Value = "Created Date";
            worksheet.Cell(1, 7).Value = "Status";

            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;

            // Body
            int row = 2;
            foreach (var item in news)
            {
                worksheet.Cell(row, 1).Value = item.NewsArticleId;
                worksheet.Cell(row, 2).Value = item.NewsTitle;
                worksheet.Cell(row, 3).Value = item.Category?.CategoryName;
                worksheet.Cell(row, 4).Value = item.CreatedBy?.AccountName;
                worksheet.Cell(row, 5).Value = item.ViewCount;
                worksheet.Cell(row, 6).Value = item.CreatedDate?.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cell(row, 7).Value = item.NewsStatus == true ? "Active" : "Inactive";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "NewsReport.xlsx");
        }
    }
}
