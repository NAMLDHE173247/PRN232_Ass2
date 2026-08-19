using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using ass01.DataAccess.Repositories;
using ass01.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ass01.BusinessLogic.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IAuditLogRepository repository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogService> logger)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(string action, string entityName, string? entityId, object? beforeSnapshot, object? afterSnapshot)
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            short? userId = null;
            string? userEmail = null;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (short.TryParse(idClaim, out var parsedId))
                {
                    userId = parsedId;
                }

                userEmail = user.FindFirst(ClaimTypes.Email)?.Value 
                            ?? user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
            }

            var log = new AuditLog
            {
                UserId = userId,
                UserEmail = userEmail,
                Action = action,
                Entity = entityName,
                EntityId = entityId,
                BeforeJson = beforeSnapshot != null ? JsonSerializer.Serialize(beforeSnapshot) : null,
                AfterJson = afterSnapshot != null ? JsonSerializer.Serialize(afterSnapshot) : null,
                Timestamp = DateTime.UtcNow
            };

            await _repository.AddLogAsync(log);
        }
        catch (Exception ex)
        {
            // Do not break the business flow if audit logging fails
            _logger.LogError(ex, "Failed to write audit log for Action={Action}, Entity={Entity}, EntityId={EntityId}", action, entityName, entityId);
        }
    }
}
