using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ass01.BusinessLogic.DTOs.Tag;

namespace ass01.BusinessLogic.DTOs.NewsArticle;

public class NewsArticleDto
{
    [Key]
    public string NewsArticleId { get; set; } = null!;
    public string? NewsTitle { get; set; }
    public string Headline { get; set; } = null!;
    public DateTime? CreatedDate { get; set; }
    public string? NewsContent { get; set; }
    public string? NewsSource { get; set; }
    public short? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool? NewsStatus { get; set; }
    public short? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public short? UpdatedById { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int ViewCount { get; set; }
    public string? ImageUrl { get; set; }

    public List<TagDto> Tags { get; set; } = new();
}

public class CreateNewsArticleRequest
{
    [Required]
    [MaxLength(400)]
    public string NewsTitle { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string Headline { get; set; } = null!;

    [MaxLength(4000)]
    public string? NewsContent { get; set; }

    [MaxLength(400)]
    public string? NewsSource { get; set; }

    public short? CategoryId { get; set; }

    public bool? NewsStatus { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public List<int> TagIds { get; set; } = new();
}

public class UpdateNewsArticleRequest
{
    [Required]
    [MaxLength(400)]
    public string NewsTitle { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string Headline { get; set; } = null!;

    [MaxLength(4000)]
    public string? NewsContent { get; set; }

    [MaxLength(400)]
    public string? NewsSource { get; set; }

    public short? CategoryId { get; set; }

    public bool? NewsStatus { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public List<int> TagIds { get; set; } = new();
}
