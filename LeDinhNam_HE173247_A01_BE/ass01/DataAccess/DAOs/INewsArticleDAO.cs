using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.Models;

namespace ass01.DataAccess.DAOs;

public interface INewsArticleDAO
{
    Task<List<NewsArticle>> GetNewsArticlesAsync(bool isStaff);
    Task<NewsArticle?> GetNewsArticleByIdAsync(string id, bool isStaff);
    Task AddNewsArticleAsync(NewsArticle article);
    Task UpdateNewsArticleAsync(NewsArticle article);
    Task DeleteNewsArticleAsync(NewsArticle article);
    Task<List<NewsArticle>> GetRelatedNewsArticlesAsync(string articleId, short? categoryId, List<int> tagIds);
    Task<List<NewsArticle>> GetReportArticlesAsync(DateTime startDate, DateTime endDate);
    Task IncrementViewCountAsync(string id);
}
