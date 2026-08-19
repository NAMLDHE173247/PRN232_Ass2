using System.ComponentModel.DataAnnotations;

namespace ass01.BusinessLogic.DTOs.Account;

public class UpdateProfileRequest
{
    [Required]
    [StringLength(100)]
    public string AccountName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(70)]
    public string AccountEmail { get; set; } = null!;
}
