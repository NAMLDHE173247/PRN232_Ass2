using System.ComponentModel.DataAnnotations;

namespace ass01.BusinessLogic.DTOs.Category;

public class CategoryDto
{
    public short CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string CategoryDescription { get; set; } = null!;
    public short? ParentCategoryId { get; set; }
    public bool? IsActive { get; set; }
    public int ArticleCount { get; set; }
}

public class CreateCategoryRequest
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    [Required]
    [StringLength(250)]
    public string CategoryDescription { get; set; } = null!;

    public short? ParentCategoryId { get; set; }

    public bool? IsActive { get; set; }
}

public class UpdateCategoryRequest
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    [Required]
    [StringLength(250)]
    public string CategoryDescription { get; set; } = null!;

    public short? ParentCategoryId { get; set; }

    public bool? IsActive { get; set; }
}
