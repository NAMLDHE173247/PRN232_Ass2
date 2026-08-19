using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ass01.Models;
using ass01.BusinessLogic.DTOs.Category;
using Microsoft.EntityFrameworkCore;

namespace ass01.DataAccess.DAOs;

public class CategoryDAO : ICategoryDAO
{
    private readonly FunewsManagementContext _context;

    public CategoryDAO(FunewsManagementContext context)
    {
        _context = context;
    }

    public async Task<(List<CategoryDto> Items, int TotalCount)> GetCategoriesWithArticleCountAsync(string? searchKeyword, int? skip = null, int? top = null)
    {
        var query = _context.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var keyword = searchKeyword.ToLower();
            query = query.Where(c => 
                (c.CategoryName != null && c.CategoryName.ToLower().Contains(keyword)) ||
                (c.CategoryDesciption != null && c.CategoryDesciption.ToLower().Contains(keyword))
            );
        }

        var totalCount = await query.CountAsync();

        int actualSkip = skip ?? 0;
        if (actualSkip < 0) actualSkip = 0;

        int actualTop = top ?? 20; // default page size
        if (actualTop <= 0) actualTop = 20;
        if (actualTop > 100) actualTop = 100;

        if (actualSkip > 0)
        {
            query = query.Skip(actualSkip);
        }

        query = query.Take(actualTop);

        var items = await query.Select(c => new CategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            CategoryDescription = c.CategoryDesciption ?? string.Empty,
            ParentCategoryId = c.ParentCategoryId,
            IsActive = c.IsActive,
            ArticleCount = c.NewsArticles.Count
        }).ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(short id)
    {
        return await _context.Categories
            .Include(c => c.NewsArticles)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
    }

    public async Task AddCategoryAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(Category category)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasNewsArticlesAsync(short categoryId)
    {
        return await _context.NewsArticles
            .AnyAsync(n => n.CategoryId == categoryId);
    }

    public async Task<bool> CategoryNameExistsAsync(string categoryName, short? parentCategoryId, short? excludeCategoryId = null)
    {
        var query = _context.Categories.Where(c => c.CategoryName == categoryName && c.ParentCategoryId == parentCategoryId);
        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId != excludeCategoryId.Value);
        }
        return await query.AnyAsync();
    }
}
