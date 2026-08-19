using System.Threading.Tasks;

namespace ass01.BusinessLogic.Services;

public interface IAuditLogService
{
    Task LogAsync(string action, string entityName, string? entityId, object? beforeSnapshot, object? afterSnapshot);
}
