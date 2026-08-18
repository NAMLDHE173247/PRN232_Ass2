using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;

namespace ass01_FE.Presentation.Controllers;

public class TagController : Controller
{
    private readonly TagApiService _tagApiService;

    public TagController(TagApiService tagApiService)
    {
        _tagApiService = tagApiService;
    }

    private bool IsStaff()
    {
        return HttpContext.Session.GetString("UserRole") == "Staff";
    }

    private string GetToken()
    {
        return HttpContext.Session.GetString("AccessToken") ?? string.Empty;
    }

    public async Task<IActionResult> Index()
    {
        if (!IsStaff())
        {
            return RedirectToAction("Index", "Home");
        }

        var token = GetToken();
        var tags = await _tagApiService.GetTagsAsync(token);
        
        return View(tags);
    }

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        if (!IsStaff()) return Unauthorized();
        var token = GetToken();
        var tags = await _tagApiService.GetTagsAsync(token);
        return Json(tags);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object model)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");
        
        var token = GetToken();
        var response = await _tagApiService.CreateTagAsync(token, model);
        
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
        
        var token = GetToken();
        var response = await _tagApiService.UpdateTagAsync(token, id, model);
        
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
        
        var token = GetToken();
        var response = await _tagApiService.DeleteTagAsync(token, id);
        
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
        var token = GetToken(); // Wait, token isn't strictly required if it's AllowAnonymous in Backend, but we can pass it
        var news = await _tagApiService.GetNewsForTagAsync(token, id);
        return Json(news);
    }
}
