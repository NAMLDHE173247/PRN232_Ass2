using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using ass01_FE.Presentation.Models.Dashboard;

namespace ass01_FE.Infrastructure.Clients;

public class AnalyticsApiClient
{
    public HttpClient Client { get; }

    public AnalyticsApiClient(HttpClient client)
    {
        Client = client;
    }

    private string BuildQueryString(DashboardFilterViewModel filter)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        
        if (filter.StartDate.HasValue)
            query["StartDate"] = filter.StartDate.Value.ToString("yyyy-MM-ddTHH:mm:ss");
        
        if (filter.EndDate.HasValue)
            query["EndDate"] = filter.EndDate.Value.ToString("yyyy-MM-ddTHH:mm:ss");
            
        if (filter.CategoryId.HasValue)
            query["CategoryId"] = filter.CategoryId.Value.ToString();
            
        if (filter.Status.HasValue)
            query["Status"] = filter.Status.Value.ToString();
            
        if (filter.AuthorId.HasValue)
            query["AuthorId"] = filter.AuthorId.Value.ToString();
            
        return query.ToString();
    }

    public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync(DashboardFilterViewModel filter)
    {
        var queryString = BuildQueryString(filter);
        var url = string.IsNullOrEmpty(queryString) ? "api/analytics/dashboard" : $"api/analytics/dashboard?{queryString}";
        
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<DashboardStatisticsDto>() ?? new DashboardStatisticsDto();
    }

    public async Task<List<TrendingArticleDto>> GetTrendingArticlesAsync(DashboardFilterViewModel filter)
    {
        var queryString = BuildQueryString(filter);
        var url = string.IsNullOrEmpty(queryString) ? "api/analytics/trending" : $"api/analytics/trending?{queryString}";
        
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<TrendingArticleDto>>() ?? new List<TrendingArticleDto>();
    }

    public async Task<byte[]> ExportAnalyticsReportAsync(DashboardFilterViewModel filter)
    {
        var queryString = BuildQueryString(filter);
        var url = string.IsNullOrEmpty(queryString) ? "api/analytics/export" : $"api/analytics/export?{queryString}";
        
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<List<ass01_FE.Presentation.Models.News.NewsArticleDto>> GetRecommendedArticlesAsync(string newsId)
    {
        var response = await Client.GetAsync($"api/recommend/{newsId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<ass01_FE.Presentation.Models.News.NewsArticleDto>>() ?? new List<ass01_FE.Presentation.Models.News.NewsArticleDto>();
        }
        return new List<ass01_FE.Presentation.Models.News.NewsArticleDto>();
    }
}
