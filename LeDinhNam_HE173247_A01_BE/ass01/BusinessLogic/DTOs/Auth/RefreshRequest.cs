using System.ComponentModel.DataAnnotations;

namespace ass01.BusinessLogic.DTOs.Auth;

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
