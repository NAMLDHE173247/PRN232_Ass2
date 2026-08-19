using System.ComponentModel.DataAnnotations;

namespace ass01.BusinessLogic.DTOs.Tag;

public class TagDto
{
    public int TagId { get; set; }
    public string? TagName { get; set; }
    public string? Note { get; set; }
}

public class CreateTagRequest
{
    [Required]
    [StringLength(50)]
    public string TagName { get; set; } = null!;

    [StringLength(200)]
    public string? Note { get; set; }
}

public class UpdateTagRequest
{
    [Required]
    [StringLength(50)]
    public string TagName { get; set; } = null!;

    [StringLength(200)]
    public string? Note { get; set; }
}
