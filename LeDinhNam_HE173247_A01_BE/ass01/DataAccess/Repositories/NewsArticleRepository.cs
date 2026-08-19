using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.DataAccess.DAOs;
using ass01.Models;

namespace ass01.DataAccess.Repositories;

public class NewsArticleRepository : INewsArticleRepository
{
    private readonly INewsArticleDAO _dao;

    public NewsArticleRepository(INewsArticleDAO dao)
    {
        _dao = dao;
    }

    public Task<List<NewsArticle>> GetNewsArticlesAsync(bool isStaff) => _dao.GetNewsArticlesAsync(isStaff);

    public Task<NewsArticle?> GetNewsArticleByIdAsync(string id, bool isStaff) => _dao.GetNewsArticleByIdAsync(id, isStaff);

    public Task AddNewsArticleAsync(NewsArticle article) => _dao.AddNewsArticleAsync(article);

    public Task UpdateNewsArticleAsync(NewsArticle article) => _dao.UpdateNewsArticleAsync(article);

    public Task DeleteNewsArticleAsync(NewsArticle article) => _dao.DeleteNewsArticleAsync(article);

    public Task<List<NewsArticle>> GetRelatedNewsArticlesAsync(string articleId, short? categoryId, List<int> tagIds)
        => _dao.GetRelatedNewsArticlesAsync(articleId, categoryId, tagIds);

    public Task<List<NewsArticle>> GetReportArticlesAsync(DateTime startDate, DateTime endDate)
        => _dao.GetReportArticlesAsync(startDate, endDate);

    public Task IncrementViewCountAsync(string id) => _dao.IncrementViewCountAsync(id);
}
