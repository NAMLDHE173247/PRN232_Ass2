using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;
using ass01_FE.Presentation.Models.Account;

namespace ass01_FE.Presentation.Controllers;

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

    private string GetToken()
    {
        return HttpContext.Session.GetString("AccessToken") ?? string.Empty;
    }

    public async Task<IActionResult> Index(string? keyword, short? role)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Keyword = keyword;
        ViewBag.Role = role;

        var token = GetToken();
        var accounts = await _accountApiService.GetAccountsAsync(token, keyword, role);

        return View(accounts);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(string? keyword, short? role)
    {
        if (!IsAdmin()) return Unauthorized();
        var token = GetToken();
        var accounts = await _accountApiService.GetAccountsAsync(token, keyword, role);
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

        var token = GetToken();
        var (success, message) = await _accountApiService.CreateAccountAsync(token, model);
        
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

        var token = GetToken();
        var (success, message) = await _accountApiService.UpdateAccountAsync(token, model);
        
        if (success)
            return Ok(new { message });
        else
            return BadRequest(new { message });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(short id)
    {
        if (!IsAdmin()) return Unauthorized("Admin access required.");
        
        var token = GetToken();
        var (success, message) = await _accountApiService.DeleteAccountAsync(token, id);
        
        if (success)
            return Ok(new { message });
        else
            return BadRequest(new { message });
    }
}
