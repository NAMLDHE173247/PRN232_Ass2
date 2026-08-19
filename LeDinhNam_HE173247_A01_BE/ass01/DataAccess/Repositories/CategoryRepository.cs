using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Category;
using ass01.DataAccess.DAOs;
using ass01.Models;

namespace ass01.DataAccess.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ICategoryDAO _dao;

    public CategoryRepository(ICategoryDAO dao)
    {
        _dao = dao;
    }

    public Task<(List<CategoryDto> Items, int TotalCount)> GetCategoriesWithArticleCountAsync(string? searchKeyword, int? skip = null, int? top = null)
        => _dao.GetCategoriesWithArticleCountAsync(searchKeyword, skip, top);

    public Task<List<Category>> GetCategoriesAsync()
        => _dao.GetCategoriesAsync();

    public Task<Category?> GetCategoryByIdAsync(short id)
        => _dao.GetCategoryByIdAsync(id);

    public Task AddCategoryAsync(Category category)
        => _dao.AddCategoryAsync(category);

    public Task UpdateCategoryAsync(Category category)
        => _dao.UpdateCategoryAsync(category);

    public Task DeleteCategoryAsync(Category category)
        => _dao.DeleteCategoryAsync(category);

    public Task<bool> HasNewsArticlesAsync(short categoryId)
        => _dao.HasNewsArticlesAsync(categoryId);

    public Task<bool> CategoryNameExistsAsync(string categoryName, short? parentCategoryId, short? excludeCategoryId = null)
        => _dao.CategoryNameExistsAsync(categoryName, parentCategoryId, excludeCategoryId);
}
