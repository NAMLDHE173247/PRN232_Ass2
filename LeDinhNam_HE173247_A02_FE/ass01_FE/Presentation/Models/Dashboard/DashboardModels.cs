using System;
using System.Collections.Generic;

namespace ass01_FE.Presentation.Models.Dashboard;

// DTOs that map to the Analytics API responses
public class DashboardStatisticsDto
{
    public int TotalArticles { get; set; }
    public int ActiveArticles { get; set; }
    public int InactiveArticles { get; set; }
    public List<CategoryArticleCountDto> ArticlesByCategory { get; set; } = new();
    public List<StatusArticleCountDto> ArticlesByStatus { get; set; } = new();
    public List<AuthorArticleCountDto> ArticlesByAuthor { get; set; } = new();
}

public class CategoryArticleCountDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class StatusArticleCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AuthorArticleCountDto
{
    public string AuthorName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TrendingArticleDto
{
    public string NewsTitle { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

// ViewModels for the MVC View
public class DashboardFilterViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public short? CategoryId { get; set; }
    public short? Status { get; set; }
    public short? AuthorId { get; set; }
}

public class DashboardViewModel
{
    public DashboardFilterViewModel CurrentFilter { get; set; } = new();
    public DashboardStatisticsDto Statistics { get; set; } = new();
    public List<TrendingArticleDto> TrendingArticles { get; set; } = new();
}
