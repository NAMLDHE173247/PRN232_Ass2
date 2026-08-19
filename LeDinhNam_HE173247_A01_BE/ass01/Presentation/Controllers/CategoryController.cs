using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Category;
using ass01.BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ass01.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Staff")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] string? search, [FromQuery] int? skip = null, [FromQuery] int? top = null)
    {
        var result = await _categoryService.GetCategoriesAsync(search, skip, top);
        return Ok(new { value = result.Items, count = result.TotalCount });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(short id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound(new { message = "Category not found." });

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = await _categoryService.CreateCategoryAsync(request);
            return CreatedAtAction(nameof(GetCategoryById), new { id = created.CategoryId }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(short id, [FromBody] UpdateCategoryRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _categoryService.UpdateCategoryAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Category not found." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(short id)
    {
        try
        {
            await _categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Category not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
