using System;

namespace ass01.Models;

public class AuditLog
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
