using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ass01.Models;
using Microsoft.EntityFrameworkCore;

namespace ass01.DataAccess.DAOs;

public class AuditLogDAO : IAuditLogDAO
{
    private readonly FunewsManagementContext _context;

    public AuditLogDAO(FunewsManagementContext context)
    {
        _context = context;
    }

    public async Task AddLogAsync(AuditLog log)
    {
        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetLogsAsync(short? userId, string? entity, string? action, int skip = 0, int top = 50)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrEmpty(entity))
            query = query.Where(a => a.Entity == entity);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(top)
            .ToListAsync();
    }

    public async Task<int> GetLogsCountAsync(short? userId, string? entity, string? action)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrEmpty(entity))
            query = query.Where(a => a.Entity == entity);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        return await query.CountAsync();
    }
}
