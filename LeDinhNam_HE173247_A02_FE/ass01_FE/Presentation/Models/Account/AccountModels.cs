using System.ComponentModel.DataAnnotations;

namespace ass01_FE.Presentation.Models.Account;

public class AccountDto
{
    public short AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? AccountEmail { get; set; }
    public short? AccountRole { get; set; }
}

public class CreateAccountViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string AccountEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 6)]
    public string AccountPassword { get; set; } = string.Empty;

    [Required]
    public short AccountRole { get; set; }
}

public class UpdateAccountViewModel
{
    public short AccountId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string AccountEmail { get; set; } = string.Empty;

    [Required]
    public short AccountRole { get; set; }
}
