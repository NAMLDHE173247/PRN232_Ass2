using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Category;
using ass01.DataAccess.Repositories;
using ass01.Models;

namespace ass01.BusinessLogic.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IAuditLogService _auditLogService;

    public CategoryService(ICategoryRepository repository, IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    public async Task<(List<CategoryDto> Items, int TotalCount)> GetCategoriesAsync(string? searchKeyword, int? skip = null, int? top = null)
    {
        return await _repository.GetCategoriesWithArticleCountAsync(searchKeyword, skip, top);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(short id)
    {
        var c = await _repository.GetCategoryByIdAsync(id);
        if (c == null) return null;

        return new CategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            CategoryDescription = c.CategoryDesciption ?? string.Empty,
            ParentCategoryId = c.ParentCategoryId,
            IsActive = c.IsActive,
            ArticleCount = c.NewsArticles?.Count ?? 0
        };
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
    {
        if (await _repository.CategoryNameExistsAsync(request.CategoryName, request.ParentCategoryId))
        {
            throw new ArgumentException("A category with this name already exists under the specified parent category.");
        }

        var category = new Category
        {
            CategoryName = request.CategoryName,
            CategoryDesciption = request.CategoryDescription,
            ParentCategoryId = request.ParentCategoryId,
            IsActive = request.IsActive
        };

        await _repository.AddCategoryAsync(category);

        var dto = new CategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            CategoryDescription = category.CategoryDesciption ?? string.Empty,
            ParentCategoryId = category.ParentCategoryId,
            IsActive = category.IsActive
        };

        await _auditLogService.LogAsync("CREATE", "Category", category.CategoryId.ToString(), null, dto);

        return dto;
    }

    public async Task UpdateCategoryAsync(short id, UpdateCategoryRequest request)
    {
        var category = await _repository.GetCategoryByIdAsync(id);
        if (category == null)
            throw new KeyNotFoundException("Category not found.");

        if (request.ParentCategoryId == id)
            throw new ArgumentException("Parent category cannot be the same as the current category.");

        if (category.ParentCategoryId != request.ParentCategoryId)
        {
            var hasNews = await _repository.HasNewsArticlesAsync(id);
            if (hasNews)
            {
                throw new InvalidOperationException("Cannot change parent category because this category is being used by news articles.");
            }
        }

        if (await _repository.CategoryNameExistsAsync(request.CategoryName, request.ParentCategoryId, id))
        {
            throw new ArgumentException("A category with this name already exists under the specified parent category.");
        }

        // Snapshot before update
        var beforeSnapshot = new
        {
            category.CategoryId,
            category.CategoryName,
            category.CategoryDesciption,
            category.ParentCategoryId,
            category.IsActive
        };

        category.CategoryName = request.CategoryName;
        category.CategoryDesciption = request.CategoryDescription;
        category.ParentCategoryId = request.ParentCategoryId;
        category.IsActive = request.IsActive;

        await _repository.UpdateCategoryAsync(category);

        // Snapshot after update
        var afterSnapshot = new
        {
            category.CategoryId,
            category.CategoryName,
            category.CategoryDesciption,
            category.ParentCategoryId,
            category.IsActive
        };

        await _auditLogService.LogAsync("UPDATE", "Category", category.CategoryId.ToString(), beforeSnapshot, afterSnapshot);
    }

    public async Task DeleteCategoryAsync(short id)
    {
        var category = await _repository.GetCategoryByIdAsync(id);
        if (category == null)
            throw new KeyNotFoundException("Category not found.");

        var hasNews = await _repository.HasNewsArticlesAsync(id);
        if (hasNews)
            throw new InvalidOperationException("Cannot delete this category because it is linked to news articles.");

        // Snapshot before delete
        var beforeSnapshot = new
        {
            category.CategoryId,
            category.CategoryName,
            category.CategoryDesciption,
            category.ParentCategoryId,
            category.IsActive
        };

        await _repository.DeleteCategoryAsync(category);

        await _auditLogService.LogAsync("DELETE", "Category", category.CategoryId.ToString(), beforeSnapshot, null);
    }
}

