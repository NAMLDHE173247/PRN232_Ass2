using System;

namespace FUNewsManagement_AIAPI.Models;

public class TagLearningCache
{
    public int Id { get; set; }
    public string Keyword { get; set; } = null!;
    public string TagName { get; set; } = null!;
    public int SelectedCount { get; set; }
    public DateTime LastUpdated { get; set; }
}
