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

        if (result == null || string.IsNullOrEmpty(result.Token))
        {
            ViewBag.Error = "Invalid email or password.";
            return View(model);
        }

        HttpContext.Session.SetString("AccessToken", result.Token);
        HttpContext.Session.SetString("UserRole", result.Role);
        HttpContext.Session.SetString("AccountId", result.AccountId);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
