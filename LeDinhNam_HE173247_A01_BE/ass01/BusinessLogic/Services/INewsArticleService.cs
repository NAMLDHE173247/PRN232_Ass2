using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.NewsArticle;

namespace ass01.BusinessLogic.Services;

public interface INewsArticleService
{
    Task<List<NewsArticleDto>> GetNewsArticlesAsync(bool isStaff, string? keyword = null, short? categoryId = null, string? tagName = null, short? createdById = null, DateTime? startDate = null, DateTime? endDate = null, string? authorName = null, bool? newsStatus = null);
    Task<NewsArticleDto?> GetNewsArticleByIdAsync(string id, bool isStaff);
    Task<NewsArticleDto> CreateNewsArticleAsync(CreateNewsArticleRequest request, short currentUserId);
    Task UpdateNewsArticleAsync(string id, UpdateNewsArticleRequest request, short currentUserId);
    Task DeleteNewsArticleAsync(string id);
    Task<NewsArticleDto> DuplicateNewsArticleAsync(string id, short currentUserId);
    Task<List<NewsArticleDto>> GetRelatedNewsArticlesAsync(string id);
    Task IncrementViewCountAsync(string id);
}
