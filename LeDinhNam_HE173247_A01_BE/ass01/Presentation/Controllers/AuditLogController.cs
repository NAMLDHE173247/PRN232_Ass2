using System.Threading.Tasks;
using ass01.DataAccess.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ass01.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogRepository _repository;

    public AuditLogController(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] short? user,
        [FromQuery] string? entity,
        [FromQuery] string? action,
        [FromQuery] int skip = 0,
        [FromQuery] int top = 50)
    {
        var logs = await _repository.GetLogsAsync(user, entity, action, skip, top);
        var totalCount = await _repository.GetLogsCountAsync(user, entity, action);

        return Ok(new
        {
            value = logs,
            count = totalCount
        });
    }
}
