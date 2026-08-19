using System;
using System.Collections.Generic;

namespace ass01.BusinessLogic.DTOs.Report;

public class ReportRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ReportResponse
{
    public int TotalArticles { get; set; }
    public int ActiveArticles { get; set; }
    public int InactiveArticles { get; set; }
    public List<CategoryStatistic> CategoryStatistics { get; set; } = new();
    public List<AuthorStatistic> AuthorStatistics { get; set; } = new();
    public List<StatusStatistic> StatusStatistics { get; set; } = new();
    public List<ass01.BusinessLogic.DTOs.NewsArticle.NewsArticleDto> Articles { get; set; } = new();
}

public class CategoryStatistic
{
    public short? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int Count { get; set; }
}

public class AuthorStatistic
{
    public short? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public int Count { get; set; }
}

public class StatusStatistic
{
    public bool? Status { get; set; }
    public int Count { get; set; }
}
