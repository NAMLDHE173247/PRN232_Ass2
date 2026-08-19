using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.NewsArticle;
using ass01.BusinessLogic.DTOs.Tag;
using ass01.Models;
using Microsoft.AspNetCore.SignalR;
using ass01.Presentation.Hubs;
using ass01.DataAccess.Repositories;

namespace ass01.BusinessLogic.Services;

public class NewsArticleService : INewsArticleService
{
    private readonly INewsArticleRepository _newsRepo;
    private readonly ITagRepository _tagRepo;
    private readonly IAuditLogService _auditLogService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NewsArticleService(INewsArticleRepository newsRepo, ITagRepository tagRepo, IAuditLogService auditLogService, IHubContext<NotificationHub> hubContext)
    {
        _newsRepo = newsRepo;
        _tagRepo = tagRepo;
        _auditLogService = auditLogService;
        _hubContext = hubContext;
    }

    public async Task<List<NewsArticleDto>> GetNewsArticlesAsync(bool isStaff, string? keyword = null, short? categoryId = null, string? tagName = null, short? createdById = null, DateTime? startDate = null, DateTime? endDate = null, string? authorName = null, bool? newsStatus = null)
    {
        var articles = await _newsRepo.GetNewsArticlesAsync(isStaff);
        var query = articles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(a => 
                (a.NewsTitle != null && a.NewsTitle.ToLower().Contains(lowerKeyword)) ||
                (a.Headline != null && a.Headline.ToLower().Contains(lowerKeyword)) ||
                (a.NewsContent != null && a.NewsContent.ToLower().Contains(lowerKeyword))
            );
        }

        if (categoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(tagName))
        {
            var lowerTag = tagName.ToLower();
            query = query.Where(a => a.Tags.Any(t => t.TagName != null && t.TagName.ToLower().Contains(lowerTag)));
        }

        if (createdById.HasValue)
        {
            query = query.Where(a => a.CreatedById == createdById.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.CreatedDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1);
            query = query.Where(a => a.CreatedDate < endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(authorName))
        {
            var lowerAuthor = authorName.ToLower();
            query = query.Where(a => a.CreatedBy != null && a.CreatedBy.AccountName != null && a.CreatedBy.AccountName.ToLower().Contains(lowerAuthor));
        }

        if (newsStatus.HasValue)
        {
            query = query.Where(a => a.NewsStatus == newsStatus.Value);
        }

        return query.Select(MapToDto).ToList();
    }

    public async Task<NewsArticleDto?> GetNewsArticleByIdAsync(string id, bool isStaff)
    {
        var article = await _newsRepo.GetNewsArticleByIdAsync(id, isStaff);
        if (article == null) return null;
        return MapToDto(article);
    }

    public async Task<NewsArticleDto> CreateNewsArticleAsync(CreateNewsArticleRequest request, short currentUserId)
    {
        var newArticleId = DateTime.Now.Ticks.ToString();
        while (await _newsRepo.GetNewsArticleByIdAsync(newArticleId, true) != null)
        {
            newArticleId = DateTime.Now.Ticks.ToString();
        }

        var article = new NewsArticle
        {
            NewsArticleId = newArticleId,
            NewsTitle = request.NewsTitle,
            Headline = request.Headline,
            NewsContent = request.NewsContent,
            NewsSource = request.NewsSource,
            CategoryId = request.CategoryId,
            NewsStatus = request.NewsStatus,
            ImageUrl = request.ImageUrl,
            CreatedById = currentUserId,
            CreatedDate = DateTime.Now
        };

        if (request.TagIds != null && request.TagIds.Any())
        {
            var allTags = await _tagRepo.GetTagsAsync();
            var validTags = allTags.Where(t => request.TagIds.Contains(t.TagId)).ToList();
            if (validTags.Count != request.TagIds.Distinct().Count())
            {
                throw new ArgumentException("One or more tags do not exist.");
            }
            foreach (var tag in validTags)
            {
                article.Tags.Add(tag);
            }
        }

        await _newsRepo.AddNewsArticleAsync(article);

        // Fetch again to get fully loaded navigational properties
        var created = await _newsRepo.GetNewsArticleByIdAsync(newArticleId, true);
        var dto = MapToDto(created!);

        await _auditLogService.LogAsync("CREATE", "NewsArticle", newArticleId, null, dto);

        try
        {
            await _hubContext.Clients.All.SendAsync("NewsCreated", new
            {
                id = dto.NewsArticleId,
                title = dto.NewsTitle,
                createdAt = dto.CreatedDate,
                category = dto.CategoryName,
                author = dto.CreatedByName
            });
        }
        catch (Exception ex)
        {
            // Log the exception but do not fail the request
            Console.WriteLine($"SignalR broadcast failed: {ex.Message}");
        }

        return dto;
    }

    public async Task UpdateNewsArticleAsync(string id, UpdateNewsArticleRequest request, short currentUserId)
    {
        var article = await _newsRepo.GetNewsArticleByIdAsync(id, true);
        if (article == null)
            throw new KeyNotFoundException("NewsArticle not found.");

        // Snapshot before update
        var beforeSnapshot = new
        {
            article.NewsArticleId,
            article.NewsTitle,
            article.Headline,
            article.NewsContent,
            article.NewsSource,
            article.CategoryId,
            article.NewsStatus,
            article.ImageUrl,
            article.ViewCount,
            TagIds = article.Tags.Select(t => t.TagId).ToList()
        };

        article.NewsTitle = request.NewsTitle;
        article.Headline = request.Headline;
        article.NewsContent = request.NewsContent;
        article.NewsSource = request.NewsSource;
        article.CategoryId = request.CategoryId;
        article.NewsStatus = request.NewsStatus;
        if (request.ImageUrl != null)
        {
            article.ImageUrl = request.ImageUrl;
        }
        article.UpdatedById = currentUserId;
        article.ModifiedDate = DateTime.Now;

        if (request.TagIds != null)
        {
            var allTags = await _tagRepo.GetTagsAsync();
            var newTags = allTags.Where(t => request.TagIds.Contains(t.TagId)).ToList();
            
            if (newTags.Count != request.TagIds.Distinct().Count())
            {
                throw new ArgumentException("One or more tags do not exist.");
            }

            article.Tags.Clear();
            foreach (var t in newTags)
            {
                article.Tags.Add(t);
            }
        }

        await _newsRepo.UpdateNewsArticleAsync(article);

        // Snapshot after update
        var afterSnapshot = new
        {
            article.NewsArticleId,
            article.NewsTitle,
            article.Headline,
            article.NewsContent,
            article.NewsSource,
            article.CategoryId,
            article.NewsStatus,
            article.ImageUrl,
            article.ViewCount,
            TagIds = article.Tags.Select(t => t.TagId).ToList()
        };

        await _auditLogService.LogAsync("UPDATE", "NewsArticle", article.NewsArticleId, beforeSnapshot, afterSnapshot);
    }

    public async Task DeleteNewsArticleAsync(string id)
    {
        var article = await _newsRepo.GetNewsArticleByIdAsync(id, true);
        if (article == null)
            throw new KeyNotFoundException("NewsArticle not found.");

        // Snapshot before delete
        var beforeSnapshot = new
        {
            article.NewsArticleId,
            article.NewsTitle,
            article.Headline,
            article.NewsContent,
            article.NewsSource,
            article.CategoryId,
            article.NewsStatus,
            article.ImageUrl,
            article.ViewCount,
            TagIds = article.Tags.Select(t => t.TagId).ToList()
        };

        article.Tags.Clear();
        await _newsRepo.UpdateNewsArticleAsync(article);

        await _newsRepo.DeleteNewsArticleAsync(article);

        await _auditLogService.LogAsync("DELETE", "NewsArticle", article.NewsArticleId, beforeSnapshot, null);
    }

    public async Task<NewsArticleDto> DuplicateNewsArticleAsync(string id, short currentUserId)
    {
        var original = await _newsRepo.GetNewsArticleByIdAsync(id, true);
        if (original == null)
            throw new KeyNotFoundException("Original NewsArticle not found.");

        var newArticleId = DateTime.Now.Ticks.ToString();
        while (await _newsRepo.GetNewsArticleByIdAsync(newArticleId, true) != null)
        {
            newArticleId = DateTime.Now.Ticks.ToString();
        }

        var duplicated = new NewsArticle
        {
            NewsArticleId = newArticleId,
            NewsTitle = original.NewsTitle,
            Headline = original.Headline,
            NewsContent = original.NewsContent,
            NewsSource = original.NewsSource,
            CategoryId = original.CategoryId,
            NewsStatus = original.NewsStatus,
            ImageUrl = original.ImageUrl,
            CreatedById = currentUserId,
            CreatedDate = DateTime.Now
        };

        foreach (var tag in original.Tags)
        {
            duplicated.Tags.Add(tag);
        }

        await _newsRepo.AddNewsArticleAsync(duplicated);

        var created = await _newsRepo.GetNewsArticleByIdAsync(newArticleId, true);
        var dto = MapToDto(created!);

        await _auditLogService.LogAsync("CREATE", "NewsArticle", newArticleId, null, dto);

        try
        {
            await _hubContext.Clients.All.SendAsync("NewsCreated", new
            {
                id = dto.NewsArticleId,
                title = dto.NewsTitle,
                createdAt = dto.CreatedDate,
                category = dto.CategoryName,
                author = dto.CreatedByName
            });
        }
        catch (Exception ex)
        {
            // Log the exception but do not fail the request
            Console.WriteLine($"SignalR broadcast failed: {ex.Message}");
        }

        return dto;
    }

    public async Task<List<NewsArticleDto>> GetRelatedNewsArticlesAsync(string id)
    {
        var article = await _newsRepo.GetNewsArticleByIdAsync(id, true);
        if (article == null)
            throw new KeyNotFoundException("NewsArticle not found.");

        var tagIds = article.Tags.Select(t => t.TagId).ToList();
        
        var related = await _newsRepo.GetRelatedNewsArticlesAsync(id, article.CategoryId, tagIds);

        return related.Select(MapToDto).ToList();
    }

    public Task IncrementViewCountAsync(string id)
    {
        return _newsRepo.IncrementViewCountAsync(id);
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
            ViewCount = a.ViewCount,
            ImageUrl = a.ImageUrl,
            Tags = a.Tags.Select(t => new TagDto
            {
                TagId = t.TagId,
                TagName = t.TagName,
                Note = t.Note
            }).ToList()
        };
    }
}

