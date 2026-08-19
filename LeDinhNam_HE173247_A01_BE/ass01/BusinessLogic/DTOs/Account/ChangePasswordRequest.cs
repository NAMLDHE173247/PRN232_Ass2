using System.ComponentModel.DataAnnotations;

namespace ass01.BusinessLogic.DTOs.Account;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [StringLength(70, MinimumLength = 6)]
    public string NewPassword { get; set; } = null!;
}
