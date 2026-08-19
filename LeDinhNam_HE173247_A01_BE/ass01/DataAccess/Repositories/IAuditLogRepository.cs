using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.Models;

namespace ass01.DataAccess.Repositories;

public interface IAuditLogRepository
{
    Task AddLogAsync(AuditLog log);
    Task<List<AuditLog>> GetLogsAsync(short? userId, string? entity, string? action, int skip = 0, int top = 50);
    Task<int> GetLogsCountAsync(short? userId, string? entity, string? action);
}
