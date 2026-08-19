using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;
using ass01_FE.BusinessLogic.Services;

using ass01_FE.Infrastructure.Filters;
using ass01_FE.Infrastructure.Clients;

namespace ass01_FE.Presentation.Controllers;

[RoleAuthorize("Staff")]
public class StaffNewsController : Controller
{
    private readonly NewsApiService _newsApiService;
    private readonly CategoryApiService _categoryApiService;
    private readonly TagApiService _tagApiService;
    private readonly NewsViewService _newsViewService;
    private readonly AiApiClient _aiApiClient;

    public StaffNewsController(NewsApiService newsApiService, CategoryApiService categoryApiService, TagApiService tagApiService, NewsViewService newsViewService, AiApiClient aiApiClient)
    {
        _newsApiService = newsApiService;
        _categoryApiService = categoryApiService;
        _tagApiService = tagApiService;
        _newsViewService = newsViewService;
        _aiApiClient = aiApiClient;
    }

    private bool IsStaff()
    {
        return HttpContext.Session.GetString("UserRole") == "Staff";
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

        var data = await _newsViewService.GetStaffNewsDataAsync(keyword, categoryId, tagName, startDate, endDate, authorName, newsStatus, skip, top);
        
        ViewBag.TotalCount = data.Count;
        
        // Also fetch categories and tags for the filter dropdowns and create/edit modal
        ViewBag.Categories = data.Categories;
        ViewBag.Tags = data.Tags;
        ViewBag.IsOffline = data.IsOffline;

        return View(data.Items);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(string? keyword, short? categoryId, string? tagName, DateTime? startDate, DateTime? endDate, string? authorName, bool? newsStatus, int skip = 0, int top = 10)
    {
        if (!IsStaff()) return Unauthorized();

        var (items, count) = await _newsApiService.GetStaffNewsAsync(keyword, categoryId, tagName, startDate, endDate, authorName, newsStatus, skip, top);
        return Json(new { items, count });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object model)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");

        var response = await _newsApiService.CreateNewsArticleAsync(model);
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "News created successfully." });
        var error = await response.Content.ReadAsStringAsync();
        return BadRequest(new { message = error });
    }

    [HttpPut]
    public async Task<IActionResult> Update(string id, [FromBody] object model)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");

        var response = await _newsApiService.UpdateNewsArticleAsync(id, model);
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "News updated successfully." });
        var error = await response.Content.ReadAsStringAsync();
        return BadRequest(new { message = error });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");

        var response = await _newsApiService.DeleteNewsArticleAsync(id);
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "News deleted successfully." });
        var error = await response.Content.ReadAsStringAsync();
        return BadRequest(new { message = error });
    }

    [HttpPost]
    public async Task<IActionResult> Duplicate(string id)
    {
        var response = await _newsApiService.DuplicateNewsArticleAsync(id);
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "News duplicated successfully." });
        var error = await response.Content.ReadAsStringAsync();
        return BadRequest(new { message = error });
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return BadRequest(new { message = "No valid image file uploaded." });
        }

        var imageUrl = await _newsApiService.UploadNewsImageAsync(imageFile);
        if (imageUrl != null)
        {
            return Ok(new { imageUrl });
        }
        return BadRequest(new { message = "Failed to upload image." });
    }

    [HttpPost]
    public async Task<IActionResult> SuggestTags([FromBody] ass01_FE.Presentation.Models.News.SuggestTagsRequestModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Content))
        {
            return BadRequest(new { message = "Please enter article content before requesting tag suggestions." });
        }

        var response = await _aiApiClient.SuggestTagsAsync(model.Content);
        if (response != null && response.Tags != null)
        {
            return Ok(new { tags = response.Tags });
        }
        return BadRequest(new { message = "AI API is currently unavailable." });
    }

    [HttpPost]
    public async Task<IActionResult> RecordSelectedSuggestedTagsAsync([FromBody] ass01_FE.Presentation.Models.News.LearnTagRequestModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Keyword) || string.IsNullOrWhiteSpace(model.TagName))
        {
            return BadRequest(new { message = "Keyword and TagName are required." });
        }

        var success = await _aiApiClient.RecordSelectedSuggestedTagsAsync(model.Keyword, model.TagName);
        if (success)
        {
            return Ok(new { message = "Tag learned successfully." });
        }
        return BadRequest(new { message = "Failed to learn tag." });
    }
}

