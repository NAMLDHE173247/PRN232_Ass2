using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Tag;

namespace ass01.BusinessLogic.Services;

public interface ITagService
{
    Task<List<TagDto>> GetTagsAsync(string? searchKeyword);
    Task<TagDto?> GetTagByIdAsync(int id);
    Task<TagDto> CreateTagAsync(CreateTagRequest request);
    Task UpdateTagAsync(int id, UpdateTagRequest request);
    Task DeleteTagAsync(int id);
    Task<List<object>> GetNewsArticlesByTagAsync(int tagId);
}
