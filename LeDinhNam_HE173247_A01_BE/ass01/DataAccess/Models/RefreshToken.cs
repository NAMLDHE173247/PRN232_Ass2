using System;

namespace ass01.Models;

public class RefreshToken
{
    public int Id { get; set; }

    public short AccountId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public virtual SystemAccount Account { get; set; } = null!;
}
