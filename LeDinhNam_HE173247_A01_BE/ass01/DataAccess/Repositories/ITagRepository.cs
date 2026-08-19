using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.Models;

namespace ass01.DataAccess.Repositories;

public interface ITagRepository
{
    Task<List<Tag>> GetTagsAsync();
    Task<Tag?> GetTagByIdAsync(int id);
    Task AddTagAsync(Tag tag);
    Task UpdateTagAsync(Tag tag);
    Task DeleteTagAsync(Tag tag);
    Task<bool> TagNameExistsAsync(string tagName, int? excludeTagId = null);
    Task<bool> IsTagUsedAsync(int tagId);
    Task<List<NewsArticle>> GetNewsArticlesByTagAsync(int tagId);
}
