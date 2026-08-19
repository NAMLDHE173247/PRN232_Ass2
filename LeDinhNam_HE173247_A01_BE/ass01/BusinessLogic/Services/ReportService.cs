using System;
using System.Linq;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.NewsArticle;
using ass01.BusinessLogic.DTOs.Report;
using ass01.BusinessLogic.DTOs.Tag;
using ass01.DataAccess.Repositories;
using ass01.Models;

namespace ass01.BusinessLogic.Services;

public class ReportService : IReportService
{
    private readonly INewsArticleRepository _newsRepo;

    public ReportService(INewsArticleRepository newsRepo)
    {
        _newsRepo = newsRepo;
    }

    public async Task<ReportResponse> GetReportAsync(ReportRequest request)
    {
        if (request.StartDate > request.EndDate)
        {
            throw new ArgumentException("StartDate must be less than or equal to EndDate");
        }

        var articles = await _newsRepo.GetReportArticlesAsync(request.StartDate, request.EndDate);

        var response = new ReportResponse
        {
            TotalArticles = articles.Count,
            ActiveArticles = articles.Count(a => a.NewsStatus == true),
            InactiveArticles = articles.Count(a => a.NewsStatus == false),
            
            CategoryStatistics = articles
                .GroupBy(a => a.CategoryId)
                .Select(g => new CategoryStatistic
                {
                    CategoryId = g.Key,
                    CategoryName = g.First().Category?.CategoryName,
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Count)
                .ToList(),
                
            AuthorStatistics = articles
                .GroupBy(a => a.CreatedById)
                .Select(g => new AuthorStatistic
                {
                    AuthorId = g.Key,
                    AuthorName = g.First().CreatedBy?.AccountName,
                    Count = g.Count()
                })
                .OrderByDescending(a => a.Count)
                .ToList(),
                
            StatusStatistics = articles
                .GroupBy(a => a.NewsStatus)
                .Select(g => new StatusStatistic
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(s => s.Count)
                .ToList(),
                
            Articles = articles.Select(MapToDto).ToList()
        };

        return response;
    }

    private static NewsArticleDto MapToDto(NewsArticle a)
    {
        return new NewsArticleDto
        {
            NewsArticleId = a.NewsArticleId,
            NewsTitle = a.NewsTitle,
            Headline = a.Headline,
            CreatedDate = a.CreatedDate,
            NewsContent = a.NewsContent,
            NewsSource = a.NewsSource,
            CategoryId = a.CategoryId,
            CategoryName = a.Category?.CategoryName,
            NewsStatus = a.NewsStatus,
            CreatedById = a.CreatedById,
            CreatedByName = a.CreatedBy?.AccountName,
            UpdatedById = a.UpdatedById,
            UpdatedByName = a.UpdatedBy?.AccountName,
            ModifiedDate = a.ModifiedDate,
            Tags = a.Tags.Select(t => new TagDto
            {
                TagId = t.TagId,
                TagName = t.TagName,
                Note = t.Note
            }).ToList()
        };
    }
}
