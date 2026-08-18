using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;
using ass01_FE.BusinessLogic.Services;

namespace ass01_FE.Presentation.Controllers;

public class StaffNewsController : Controller
{
    private readonly NewsApiService _newsApiService;
    private readonly CategoryApiService _categoryApiService;
    private readonly TagApiService _tagApiService;
    private readonly NewsViewService _newsViewService;

    public StaffNewsController(NewsApiService newsApiService, CategoryApiService categoryApiService, TagApiService tagApiService, NewsViewService newsViewService)
    {
        _newsApiService = newsApiService;
        _categoryApiService = categoryApiService;
        _tagApiService = tagApiService;
        _newsViewService = newsViewService;
    }

    private bool IsStaff()
    {
        return HttpContext.Session.GetString("UserRole") == "Staff";
    }

    private string GetToken()
    {
        return HttpContext.Session.GetString("AccessToken") ?? string.Empty;
    }

    public async Task<IActionResult> Index(string? keyword, short? categoryId, string? tagName, DateTime? startDate, DateTime? endDate, string? authorName, bool? newsStatus, int skip = 0, int top = 10)
    {
        if (!IsStaff())
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Keyword = keyword;
        ViewBag.CategoryId = categoryId;
        ViewBag.TagName = tagName;
        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
        ViewBag.AuthorName = authorName;
        ViewBag.NewsStatus = newsStatus;
        ViewBag.Skip = skip;
        ViewBag.Top = top;

        var token = GetToken();
        var data = await _newsViewService.GetStaffNewsDataAsync(token, keyword, categoryId, tagName, startDate, endDate, authorName, newsStatus, skip, top);
        
        ViewBag.TotalCount = data.Count;
        
        // Also fetch categories and tags for the filter dropdowns and create/edit modal
        ViewBag.Categories = data.Categories;
        ViewBag.Tags = data.Tags;

        return View(data.Items);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(string? keyword, short? categoryId, string? tagName, DateTime? startDate, DateTime? endDate, string? authorName, bool? newsStatus, int skip = 0, int top = 10)
    {
        if (!IsStaff()) return Unauthorized();
        var token = GetToken();
        var (items, count) = await _newsApiService.GetStaffNewsAsync(token, keyword, categoryId, tagName, startDate, endDate, authorName, newsStatus, skip, top);
        return Json(new { items, count });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object model)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");
        var token = GetToken();
        var response = await _newsApiService.CreateNewsArticleAsync(token, model);
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "News created successfully." });
        var error = await response.Content.ReadAsStringAsync();
        return BadRequest(new { message = error });
    }

    [HttpPut]
    public async Task<IActionResult> Update(string id, [FromBody] object model)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");
        var token = GetToken();
        var response = await _newsApiService.UpdateNewsArticleAsync(token, id, model);
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "News updated successfully." });
        var error = await response.Content.ReadAsStringAsync();
        return BadRequest(new { message = error });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");
        var token = GetToken();
        var response = await _newsApiService.DeleteNewsArticleAsync(token, id);
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "News deleted successfully." });
        var error = await response.Content.ReadAsStringAsync();
        return BadRequest(new { message = error });
    }

    [HttpPost]
    public async Task<IActionResult> Duplicate(string id)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");
        var token = GetToken();
        var response = await _newsApiService.DuplicateNewsArticleAsync(token, id);
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "News duplicated successfully." });
        var error = await response.Content.ReadAsStringAsync();
        return BadRequest(new { message = error });
    }
}
