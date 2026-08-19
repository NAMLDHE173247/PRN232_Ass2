using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ass01.Models;
using Microsoft.EntityFrameworkCore;

namespace ass01.DataAccess.DAOs;

public class NewsArticleDAO : INewsArticleDAO
{
    private readonly FunewsManagementContext _context;

    public NewsArticleDAO(FunewsManagementContext context)
    {
        _context = context;
    }

    public async Task<List<NewsArticle>> GetNewsArticlesAsync(bool isStaff)
    {
        var query = _context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.Tags)
            .Include(n => n.CreatedBy)
            .Include(n => n.UpdatedBy)
            .AsQueryable();

        if (!isStaff)
        {
            query = query.Where(n => n.NewsStatus == true);
        }

        return await query.OrderByDescending(n => n.CreatedDate).ToListAsync();
    }

    public async Task<NewsArticle?> GetNewsArticleByIdAsync(string id, bool isStaff)
    {
        var query = _context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.Tags)
            .Include(n => n.CreatedBy)
            .Include(n => n.UpdatedBy)
            .AsQueryable();

        if (!isStaff)
        {
            query = query.Where(n => n.NewsStatus == true);
        }

        return await query.FirstOrDefaultAsync(n => n.NewsArticleId == id);
    }

    public async Task AddNewsArticleAsync(NewsArticle article)
    {
        await _context.NewsArticles.AddAsync(article);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateNewsArticleAsync(NewsArticle article)
    {
        _context.NewsArticles.Update(article);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteNewsArticleAsync(NewsArticle article)
    {
        _context.NewsArticles.Remove(article);
        await _context.SaveChangesAsync();
    }

    public async Task<List<NewsArticle>> GetRelatedNewsArticlesAsync(string articleId, short? categoryId, List<int> tagIds)
    {
        var query = _context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.Tags)
            .Include(n => n.CreatedBy)
            .Include(n => n.UpdatedBy)
            .Where(n => n.NewsArticleId != articleId && n.NewsStatus == true);

        return await query
            .Where(n => (categoryId.HasValue && n.CategoryId == categoryId.Value) || 
                        n.Tags.Any(t => tagIds.Contains(t.TagId)))
            .OrderByDescending(n => n.CreatedDate)
            .Take(3)
            .ToListAsync();
    }

    public async Task<List<NewsArticle>> GetReportArticlesAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.Tags)
            .Include(n => n.CreatedBy)
            .Include(n => n.UpdatedBy)
            .Where(n => n.CreatedDate >= startDate && n.CreatedDate < endDate.Date.AddDays(1))
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task IncrementViewCountAsync(string id)
    {
        await _context.NewsArticles
            .Where(n => n.NewsArticleId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ViewCount, x => x.ViewCount + 1));
    }
}
