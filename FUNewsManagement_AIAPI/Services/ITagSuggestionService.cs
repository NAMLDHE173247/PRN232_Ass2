using FUNewsManagement_AIAPI.DTOs;

namespace FUNewsManagement_AIAPI.Services;

public interface ITagSuggestionService
{
    Task<SuggestTagsResponse> SuggestTagsAsync(SuggestTagsRequest request);
    Task<bool> LearnTagAsync(LearnTagRequest request);
}
