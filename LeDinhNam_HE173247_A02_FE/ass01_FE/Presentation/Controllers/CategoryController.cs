using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;

namespace ass01_FE.Presentation.Controllers;

public class CategoryController : Controller
{
    private readonly CategoryApiService _categoryApiService;
    private readonly NewsApiService _newsApiService;

    public CategoryController(CategoryApiService categoryApiService, NewsApiService newsApiService)
    {
        _categoryApiService = categoryApiService;
        _newsApiService = newsApiService;
    }

    private bool IsStaff()
    {
        return HttpContext.Session.GetString("UserRole") == "Staff";
    }

    

    public async Task<IActionResult> Index(string? searchKeyword, int skip = 0, int top = 10)
    {
        if (!IsStaff())
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.SearchKeyword = searchKeyword;
        ViewBag.Skip = skip;
        ViewBag.Top = top;

        var result = await _categoryApiService.GetCategoriesAsync(searchKeyword, skip, top);
        
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(string? searchKeyword, int skip = 0, int top = 10)
    {
        if (!IsStaff()) return Unauthorized();

        var result = await _categoryApiService.GetCategoriesAsync(searchKeyword, skip, top);
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object model)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");

        var response = await _categoryApiService.CreateCategoryAsync(model);
        
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "Category created successfully." });
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(new { message = error });
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update(short id, [FromBody] object model)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");

        var response = await _categoryApiService.UpdateCategoryAsync(id, model);
        
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "Category updated successfully." });
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(new { message = error });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(short id)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");

        var response = await _categoryApiService.DeleteCategoryAsync(id);
        
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "Category deleted successfully." });
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(new { message = error });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetNewsForCategory(int id)
    {
        if (!IsStaff()) return Unauthorized();

        var (items, count) = await _newsApiService.GetStaffNewsAsync(categoryId: (short)id, top: 100);
        return Json(items);
    }
}

