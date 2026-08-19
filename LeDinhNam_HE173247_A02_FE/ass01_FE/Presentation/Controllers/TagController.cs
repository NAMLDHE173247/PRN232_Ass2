using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;

using ass01_FE.Infrastructure.Filters;

namespace ass01_FE.Presentation.Controllers;

[RoleAuthorize("Staff")]
public class TagController : Controller
{
    private readonly ass01_FE.Infrastructure.Services.OfflineTagService _offlineTagService;
    private readonly TagApiService _tagApiService;

    public TagController(ass01_FE.Infrastructure.Services.OfflineTagService offlineTagService, TagApiService tagApiService)
    {
        _offlineTagService = offlineTagService;
        _tagApiService = tagApiService;
    }

    private bool IsStaff()
    {
        return HttpContext.Session.GetString("UserRole") == "Staff";
    }

    

    public async Task<IActionResult> Index()
    {
        if (!IsStaff())
        {
            return RedirectToAction("Index", "Home");
        }

        var result = await _offlineTagService.GetTagsWithOfflineFallbackAsync();
        
        ViewBag.IsOffline = result.IsOffline;
        if (result.IsOffline)
        {
            ViewBag.ErrorMessage = result.ErrorMessage;
        }

        return View(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        if (!IsStaff()) return Unauthorized();

        var tags = await _tagApiService.GetTagsAsync();
        return Json(tags);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object model)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");

        var response = await _tagApiService.CreateTagAsync(model);
        
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "Tag created successfully." });
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(new { message = error });
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update(int id, [FromBody] object model)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");

        var response = await _tagApiService.UpdateTagAsync(id, model);
        
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "Tag updated successfully." });
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(new { message = error });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");

        var response = await _tagApiService.DeleteTagAsync(id);
        
        if (response.IsSuccessStatusCode)
            return Ok(new { message = "Tag deleted successfully." });
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(new { message = error });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetNewsForTag(int id)
    {
        var news = await _tagApiService.GetNewsForTagAsync(id);
        return Json(news);
    }
}

