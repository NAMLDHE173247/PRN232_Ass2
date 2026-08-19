using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Category;

namespace ass01.BusinessLogic.Services;

public interface ICategoryService
{
    Task<(List<CategoryDto> Items, int TotalCount)> GetCategoriesAsync(string? searchKeyword, int? skip = null, int? top = null);
    Task<CategoryDto?> GetCategoryByIdAsync(short id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
    Task UpdateCategoryAsync(short id, UpdateCategoryRequest request);
    Task DeleteCategoryAsync(short id);
}
