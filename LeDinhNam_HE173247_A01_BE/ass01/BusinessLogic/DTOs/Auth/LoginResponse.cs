using System;

namespace ass01.BusinessLogic.DTOs.Auth;

public class LoginResponse
{
    /// <summary>Access JWT token (alias kept for backward compat)</summary>
    public string Token { get; set; } = null!;

    /// <summary>Access JWT token (new field name per ASS2)</summary>
    public string AccessToken => Token;

    public string RefreshToken { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public short AccountId { get; set; }
}

