using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;
using ass01_FE.Presentation.Models.Account;
using ass01_FE.Infrastructure.Filters;

namespace ass01_FE.Presentation.Controllers;

[RoleAuthorize("Admin")]
public class AccountController : Controller
{
    private readonly AccountApiService _accountApiService;

    public AccountController(AccountApiService accountApiService)
    {
        _accountApiService = accountApiService;
    }

    private bool IsAdmin()
    {
        return HttpContext.Session.GetString("UserRole") == "Admin";
    }

    

    public async Task<IActionResult> Index(string? keyword, short? role)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Keyword = keyword;
        ViewBag.Role = role;

        var accounts = await _accountApiService.GetAccountsAsync(keyword, role);

        return View(accounts);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(string? keyword, short? role)
    {
        if (!IsAdmin()) return Unauthorized();

        var accounts = await _accountApiService.GetAccountsAsync(keyword, role);
        return Json(accounts);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountViewModel model)
    {
        if (!IsAdmin()) return Unauthorized("Admin access required.");
        
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid input data.");
        }

        var (success, message) = await _accountApiService.CreateAccountAsync(model);
        
        if (success)
            return Ok(new { message });
        else
            return BadRequest(new { message });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateAccountViewModel model)
    {
        if (!IsAdmin()) return Unauthorized("Admin access required.");
        
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid input data.");
        }

        var (success, message) = await _accountApiService.UpdateAccountAsync(model);
        
        if (success)
            return Ok(new { message });
        else
            return BadRequest(new { message });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(short id)
    {
        if (!IsAdmin()) return Unauthorized("Admin access required.");

        var (success, message) = await _accountApiService.DeleteAccountAsync(id);
        
        if (success)
            return Ok(new { message });
        else
            return BadRequest(new { message });
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(short id, [FromBody] ChangePasswordViewModel model)
    {
        if (!IsAdmin()) return Unauthorized("Admin access required.");
        
        if (!ModelState.IsValid)
        {
            return BadRequest("Invalid input data.");
        }

        var (success, message) = await _accountApiService.ChangePasswordAsync(id, model);
        
        if (success)
            return Ok(new { message });
        else
            return BadRequest(new { message });
    }
}

