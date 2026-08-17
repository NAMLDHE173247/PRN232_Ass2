using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ass01_FE.Presentation.Models.News; // For NewsArticleDto

namespace ass01_FE.Presentation.Models.Report;

public class ReportRequest
{
    [Required]
    public DateTime? StartDate { get; set; }
    
    [Required]
    public DateTime? EndDate { get; set; }
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

public class ReportStatisticsDto
{
    public int TotalArticles { get; set; }
    public int ActiveArticles { get; set; }
    public int InactiveArticles { get; set; }
    public List<CategoryStatistic> CategoryStatistics { get; set; } = new();
    public List<AuthorStatistic> AuthorStatistics { get; set; } = new();
    public List<StatusStatistic> StatusStatistics { get; set; } = new();
    public List<NewsArticleDto> Articles { get; set; } = new();
}

public class ReportViewModel
{
    public ReportRequest Request { get; set; } = new();
    public ReportStatisticsDto? Statistics { get; set; }
    public string? ErrorMessage { get; set; }
}
