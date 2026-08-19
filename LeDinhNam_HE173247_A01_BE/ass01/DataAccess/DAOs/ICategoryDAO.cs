using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Category;
using ass01.Models;

namespace ass01.DataAccess.DAOs;

public interface ICategoryDAO
{
    Task<(List<CategoryDto> Items, int TotalCount)> GetCategoriesWithArticleCountAsync(string? searchKeyword, int? skip = null, int? top = null);
    Task<List<Category>> GetCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(short id);
    Task AddCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(Category category);
    Task<bool> HasNewsArticlesAsync(short categoryId);
    Task<bool> CategoryNameExistsAsync(string categoryName, short? parentCategoryId, short? excludeCategoryId = null);
}
