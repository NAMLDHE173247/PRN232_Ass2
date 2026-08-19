namespace FUNewsManagement_AIAPI.DTOs;

public class SuggestTagsRequest
{
    public string Content { get; set; } = string.Empty;
}

public class SuggestTagsResponse
{
    public List<SuggestedTagDto> Tags { get; set; } = new();
}

public class SuggestedTagDto
{
    public string Name { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class LearnTagRequest
{
    public string Keyword { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
}
