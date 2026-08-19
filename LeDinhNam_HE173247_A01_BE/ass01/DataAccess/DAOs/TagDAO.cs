using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ass01.Models;
using Microsoft.EntityFrameworkCore;

namespace ass01.DataAccess.DAOs;

public class TagDAO : ITagDAO
{
    private readonly FunewsManagementContext _context;

    public TagDAO(FunewsManagementContext context)
    {
        _context = context;
    }

    public async Task<List<Tag>> GetTagsAsync()
    {
        return await _context.Tags.ToListAsync();
    }

    public async Task<Tag?> GetTagByIdAsync(int id)
    {
        return await _context.Tags.FirstOrDefaultAsync(t => t.TagId == id);
    }

    public async Task AddTagAsync(Tag tag)
    {
        await _context.Tags.AddAsync(tag);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTagAsync(Tag tag)
    {
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTagAsync(Tag tag)
    {
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> TagNameExistsAsync(string tagName, int? excludeTagId = null)
    {
        var query = _context.Tags.Where(t => t.TagName == tagName);
        if (excludeTagId.HasValue)
        {
            query = query.Where(t => t.TagId != excludeTagId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task<bool> IsTagUsedAsync(int tagId)
    {
        return await _context.Tags
            .Where(t => t.TagId == tagId)
            .SelectMany(t => t.NewsArticles)
            .AnyAsync();
    }

    public async Task<List<NewsArticle>> GetNewsArticlesByTagAsync(int tagId)
    {
        return await _context.Tags
            .Where(t => t.TagId == tagId)
            .SelectMany(t => t.NewsArticles)
            .ToListAsync();
    }
}
