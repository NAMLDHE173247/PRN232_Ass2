namespace ass01_FE.Presentation.Models.Auth;

using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string? Token { get; set; } // Backward compatibility
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string? Role { get; set; }
    public short? AccountId { get; set; }
    public string? Email { get; set; }
}
