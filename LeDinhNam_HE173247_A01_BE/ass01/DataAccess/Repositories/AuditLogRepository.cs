using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.DataAccess.DAOs;
using ass01.Models;

namespace ass01.DataAccess.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IAuditLogDAO _dao;

    public AuditLogRepository(IAuditLogDAO dao)
    {
        _dao = dao;
    }

    public Task AddLogAsync(AuditLog log) => _dao.AddLogAsync(log);

    public Task<List<AuditLog>> GetLogsAsync(short? userId, string? entity, string? action, int skip = 0, int top = 50)
        => _dao.GetLogsAsync(userId, entity, action, skip, top);

    public Task<int> GetLogsCountAsync(short? userId, string? entity, string? action)
        => _dao.GetLogsCountAsync(userId, entity, action);
}
