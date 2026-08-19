using FUNewsManagement_AIAPI.DTOs;
using FUNewsManagement_AIAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement_AIAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Staff,Admin")]
public class AiController : ControllerBase
{
    private readonly ITagSuggestionService _suggestionService;

    public AiController(ITagSuggestionService suggestionService)
    {
        _suggestionService = suggestionService;
    }

    [HttpPost("suggest-tags")]
    public async Task<IActionResult> SuggestTags([FromBody] SuggestTagsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(new { Message = "Content cannot be empty." });
        }

        var response = await _suggestionService.SuggestTagsAsync(request);
        return Ok(response);
    }

    [HttpPost("learn")]
    public async Task<IActionResult> LearnTag([FromBody] LearnTagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Keyword) || string.IsNullOrWhiteSpace(request.TagName))
        {
            return BadRequest(new { Message = "Keyword and TagName cannot be empty." });
        }

        var success = await _suggestionService.LearnTagAsync(request);
        if (!success)
        {
            return BadRequest(new { Message = "Failed to learn tag. Ensure the TagName actually exists." });
        }

        return Ok(new { Message = "Tag successfully learned/updated in cache." });
    }
}
