using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ass01_FE.Presentation.Models.Auth;
using ass01_FE.DataAccess.Services;

namespace ass01_FE.Presentation.Controllers;

public class AuthController : Controller
{
    private readonly AuthApiService _authApiService;

    public AuthController(AuthApiService authApiService)
    {
        _authApiService = authApiService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authApiService.LoginAsync(model);

        if (result == null)
        {
            ViewBag.Error = "Invalid email or password.";
            return View(model);
        }

        var tokenToUse = result.AccessToken ?? result.Token;
        if (string.IsNullOrEmpty(tokenToUse))
        {
            ViewBag.Error = "Invalid email or password.";
            return View(model);
        }

        HttpContext.Session.SetString("AccessToken", tokenToUse);
        
        if (!string.IsNullOrEmpty(result.RefreshToken))
            HttpContext.Session.SetString("RefreshToken", result.RefreshToken);
            
        if (result.ExpiresAt.HasValue)
            HttpContext.Session.SetString("ExpiresAt", result.ExpiresAt.Value.ToString("o")); // ISO-8601 UTC

        if (!string.IsNullOrEmpty(result.Role))
            HttpContext.Session.SetString("UserRole", result.Role);
            
        if (result.AccountId.HasValue)
            HttpContext.Session.SetString("AccountId", result.AccountId.Value.ToString());

        if (!string.IsNullOrEmpty(result.Email))
            HttpContext.Session.SetString("Email", result.Email);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}

