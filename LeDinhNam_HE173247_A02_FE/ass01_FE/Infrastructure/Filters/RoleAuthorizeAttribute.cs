using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace ass01_FE.Infrastructure.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _allowedRoles;

    public RoleAuthorizeAttribute(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var accessToken = context.HttpContext.Session.GetString("AccessToken");
        var userRole = context.HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(accessToken))
        {
            // Unauthorized Session -> Redirect to Login
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(userRole))
        {
            // Logged in but forbidden role -> Redirect to Home or show Forbidden
            context.Result = new RedirectToActionResult("Index", "Home", null);
            return;
        }
    }
}
