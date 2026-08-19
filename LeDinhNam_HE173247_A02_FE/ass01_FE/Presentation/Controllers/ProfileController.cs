using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;

namespace ass01_FE.Presentation.Controllers;

public class ProfileController : Controller
{
    private readonly ProfileApiService _profileApiService;

    public ProfileController(ProfileApiService profileApiService)
    {
        _profileApiService = profileApiService;
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

        var profile = await _profileApiService.GetMyProfileAsync();
        
        return View(profile);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] object model)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");

        var response = await _profileApiService.UpdateMyProfileAsync(model);
        
        if (response.IsSuccessStatusCode)
        {
            return Ok(new { message = "Profile updated successfully." });
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(new { message = error });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(short id, [FromBody] object model)
    {
        if (!IsStaff()) return Unauthorized("Staff access required.");

        var response = await _profileApiService.ChangePasswordAsync(id, model);
        
        if (response.IsSuccessStatusCode)
        {
            return Ok(new { message = "Password changed successfully." });
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(new { message = error });
        }
    }
}

