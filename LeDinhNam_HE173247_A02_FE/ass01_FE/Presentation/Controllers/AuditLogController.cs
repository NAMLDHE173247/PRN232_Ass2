using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ass01_FE.DataAccess.Services;
using ass01_FE.Infrastructure.Filters;

namespace ass01_FE.Presentation.Controllers;

[RoleAuthorize("Admin")]
public class AuditLogController : Controller
{
    private readonly AuditLogApiService _auditLogApiService;

    public AuditLogController(AuditLogApiService auditLogApiService)
    {
        _auditLogApiService = auditLogApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(short? userId, string? entity, string? actionType, int skip = 0, int top = 50)
    {
        ViewBag.UserId = userId;
        ViewBag.Entity = entity;
        ViewBag.ActionType = actionType;
        ViewBag.Skip = skip;
        ViewBag.Top = top;

        var result = await _auditLogApiService.GetAuditLogsAsync(userId, entity, actionType, skip, top);

        return View(result);
    }
}
