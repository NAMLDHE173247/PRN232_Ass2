using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Tag;
using ass01.DataAccess.Repositories;
using ass01.Models;

namespace ass01.BusinessLogic.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _repository;
    private readonly IAuditLogService _auditLogService;

    public TagService(ITagRepository repository, IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    public async Task<List<TagDto>> GetTagsAsync(string? searchKeyword)
    {
        var tags = await _repository.GetTagsAsync();
        var query = tags.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var keyword = searchKeyword.ToLower();
            query = query.Where(t => t.TagName != null && t.TagName.ToLower().Contains(keyword));
        }

        return query.Select(t => new TagDto
        {
            TagId = t.TagId,
            TagName = t.TagName,
            Note = t.Note
        }).ToList();
    }

    public async Task<TagDto?> GetTagByIdAsync(int id)
    {
        var t = await _repository.GetTagByIdAsync(id);
        if (t == null) return null;

        return new TagDto
        {
            TagId = t.TagId,
            TagName = t.TagName,
            Note = t.Note
        };
    }

    public async Task<TagDto> CreateTagAsync(CreateTagRequest request)
    {
        if (await _repository.TagNameExistsAsync(request.TagName))
            throw new ArgumentException("TagName already exists.");

        var allTags = await _repository.GetTagsAsync();
        int newId = allTags.Any() ? allTags.Max(t => t.TagId) + 1 : 1;

        var tag = new Tag
        {
            TagId = newId,
            TagName = request.TagName,
            Note = request.Note
        };

        await _repository.AddTagAsync(tag);

        var dto = new TagDto
        {
            TagId = tag.TagId,
            TagName = tag.TagName,
            Note = tag.Note
        };

        await _auditLogService.LogAsync("CREATE", "Tag", tag.TagId.ToString(), null, dto);

        return dto;
    }

    public async Task UpdateTagAsync(int id, UpdateTagRequest request)
    {
        var tag = await _repository.GetTagByIdAsync(id);
        if (tag == null)
            throw new KeyNotFoundException("Tag not found.");

        if (await _repository.TagNameExistsAsync(request.TagName, id))
            throw new ArgumentException("TagName already exists.");

        // Snapshot before update
        var beforeSnapshot = new
        {
            tag.TagId,
            tag.TagName,
            tag.Note
        };

        tag.TagName = request.TagName;
        tag.Note = request.Note;

        await _repository.UpdateTagAsync(tag);

        // Snapshot after update
        var afterSnapshot = new
        {
            tag.TagId,
            tag.TagName,
            tag.Note
        };

        await _auditLogService.LogAsync("UPDATE", "Tag", tag.TagId.ToString(), beforeSnapshot, afterSnapshot);
    }

    public async Task DeleteTagAsync(int id)
    {
        var tag = await _repository.GetTagByIdAsync(id);
        if (tag == null)
            throw new KeyNotFoundException("Tag not found.");

        if (await _repository.IsTagUsedAsync(id))
            throw new InvalidOperationException("Cannot delete this tag because it is being used by news articles.");

        // Snapshot before delete
        var beforeSnapshot = new
        {
            tag.TagId,
            tag.TagName,
            tag.Note
        };

        await _repository.DeleteTagAsync(tag);

        await _auditLogService.LogAsync("DELETE", "Tag", tag.TagId.ToString(), beforeSnapshot, null);
    }

    public async Task<List<object>> GetNewsArticlesByTagAsync(int tagId)
    {
        var articles = await _repository.GetNewsArticlesByTagAsync(tagId);
        return articles.Select(a => new
        {
            a.NewsArticleId,
            a.NewsTitle,
            a.Headline,
            a.CreatedDate,
            a.NewsStatus
        }).Cast<object>().ToList();
    }
}

