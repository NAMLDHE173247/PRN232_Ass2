using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ass01_FE.DataAccess.Services;

public class AuditLogDto
{
    public int Id { get; set; }
    public short? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = null!;
    public string Entity { get; set; } = null!;
    public string? EntityId { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AuditLogListResult
{
    public List<AuditLogDto> Value { get; set; } = new();
    public int Count { get; set; }
}

public class AuditLogApiService
{
    private readonly HttpClient _httpClient;

    public AuditLogApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var baseUrl = configuration["ApiSettings:CoreApi"] ?? throw new InvalidOperationException("ApiSettings:CoreApi is not configured.");
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<AuditLogListResult?> GetAuditLogsAsync(short? user = null, string? entity = null, string? action = null, int skip = 0, int top = 50)
    {
        var query = new List<string>();
        if (user.HasValue) query.Add($"user={user.Value}");
        if (!string.IsNullOrEmpty(entity)) query.Add($"entity={Uri.EscapeDataString(entity)}");
        if (!string.IsNullOrEmpty(action)) query.Add($"action={Uri.EscapeDataString(action)}");
        
        query.Add($"skip={skip}");
        query.Add($"top={top}");

        var qs = "?" + string.Join("&", query);
        return await _httpClient.GetFromJsonAsync<AuditLogListResult>($"/api/auditlog{qs}");
    }
}
