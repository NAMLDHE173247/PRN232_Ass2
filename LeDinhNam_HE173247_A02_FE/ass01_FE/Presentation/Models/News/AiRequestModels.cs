using System.Collections.Generic;

namespace ass01_FE.Presentation.Models.News;

public class SuggestTagsRequestModel
{
    public string Content { get; set; } = string.Empty;
}

public class LearnTagRequestModel
{
    public string Keyword { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
}
