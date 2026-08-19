using System.ComponentModel.DataAnnotations;

namespace ass01.BusinessLogic.DTOs.Account;

public class CreateAccountRequest
{
    public short AccountId { get; set; }

    [Required]
    [StringLength(100)]
    public string AccountName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(70)]
    public string AccountEmail { get; set; } = null!;

    [Required]
    public int AccountRole { get; set; }

    [Required]
    [StringLength(70, MinimumLength = 6)]
    public string AccountPassword { get; set; } = null!;
}
