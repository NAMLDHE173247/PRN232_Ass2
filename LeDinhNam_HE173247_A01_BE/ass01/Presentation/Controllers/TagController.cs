using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Tag;
using ass01.BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ass01.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Staff")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTags([FromQuery] string? search)
    {
        var tags = await _tagService.GetTagsAsync(search);
        return Ok(tags);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTagById(int id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);
        if (tag == null) return NotFound(new { message = "Tag not found." });

        return Ok(tag);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var created = await _tagService.CreateTagAsync(request);
            return CreatedAtAction(nameof(GetTagById), new { id = created.TagId }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] UpdateTagRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _tagService.UpdateTagAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Tag not found." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        try
        {
            await _tagService.DeleteTagAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Tag not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/news-articles")]
    public async Task<IActionResult> GetNewsArticlesByTag(int id)
    {
        var tag = await _tagService.GetTagByIdAsync(id);
        if (tag == null) return NotFound(new { message = "Tag not found." });

        var articles = await _tagService.GetNewsArticlesByTagAsync(id);
        return Ok(articles);
    }
}
