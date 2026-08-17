using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ass01_FE.Presentation.Models.News;

namespace ass01_FE.DataAccess.Services;

public class NewsApiService
{
    private readonly HttpClient _httpClient;

    private void AddAuthHeader(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public NewsApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? throw new System.InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        _httpClient.BaseAddress = new System.Uri(baseUrl);
    }

    public async Task<(List<NewsArticleDto> Items, int TotalCount)> GetActiveNewsAsync(string? keyword = null, int skip = 0, int top = 5)
    {
        var url = $"api/news?$skip={skip}&$top={top}&$count=true&$orderby=CreatedDate desc";
        if (!string.IsNullOrEmpty(keyword))
        {
            url += $"&keyword={System.Uri.EscapeDataString(keyword)}";
        }

        var response = await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var contentString = await response.Content.ReadAsStringAsync();
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var odataResult = System.Text.Json.JsonSerializer.Deserialize<ODataResponse<NewsArticleDto>>(contentString, options);
                if (odataResult != null && odataResult.Value != null)
                {
                    return (odataResult.Value, odataResult.Count);
                }
            }
            catch
            {
                // Fallback if backend does not wrap in OData format (e.g. returns plain array)
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var listResult = System.Text.Json.JsonSerializer.Deserialize<List<NewsArticleDto>>(contentString, options);
                if (listResult != null)
                {
                    return (listResult, listResult.Count);
                }
            }
        }
        return (new List<NewsArticleDto>(), 0);
    }

    public async Task<NewsArticleDto?> GetNewsByIdAsync(string id)
    {
        var response = await _httpClient.GetAsync($"api/news/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<NewsArticleDto>();
        }
        return null;
    }

    public async Task<List<NewsArticleDto>> GetRelatedNewsAsync(string id)
    {
        var response = await _httpClient.GetAsync($"api/news/{id}/related");
        if (response.IsSuccessStatusCode)
        {
            var articles = await response.Content.ReadFromJsonAsync<List<NewsArticleDto>>();
            return articles ?? new List<NewsArticleDto>();
        }
        return new List<NewsArticleDto>();
    }

    // --- Staff Methods ---

    public async Task<(List<NewsArticleDto> Items, int TotalCount)> GetStaffNewsAsync(
        string token, string? keyword = null, short? categoryId = null, string? tagName = null, DateTime? startDate = null, DateTime? endDate = null, string? authorName = null, bool? newsStatus = null, int skip = 0, int top = 10)
    {
        AddAuthHeader(token);
        
        var query = new List<string> { $"$skip={skip}", $"$top={top}", "$count=true", "$orderby=CreatedDate desc" };
        if (!string.IsNullOrEmpty(keyword)) query.Add($"keyword={System.Uri.EscapeDataString(keyword)}");
        if (categoryId.HasValue) query.Add($"categoryId={categoryId.Value}");
        if (!string.IsNullOrEmpty(tagName)) query.Add($"tagName={System.Uri.EscapeDataString(tagName)}");
        if (startDate.HasValue) query.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue) query.Add($"endDate={endDate.Value:yyyy-MM-dd}");
        if (!string.IsNullOrEmpty(authorName)) query.Add($"authorName={System.Uri.EscapeDataString(authorName)}");
        if (newsStatus.HasValue) query.Add($"newsStatus={newsStatus.Value.ToString().ToLower()}");

        var url = "api/news?" + string.Join("&", query);
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var contentString = await response.Content.ReadAsStringAsync();
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var odataResult = System.Text.Json.JsonSerializer.Deserialize<ODataResponse<NewsArticleDto>>(contentString, options);
                if (odataResult != null && odataResult.Value != null)
                {
                    return (odataResult.Value, odataResult.Count);
                }
            }
            catch
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var listResult = System.Text.Json.JsonSerializer.Deserialize<List<NewsArticleDto>>(contentString, options);
                if (listResult != null)
                {
                    return (listResult, listResult.Count);
                }
            }
        }
        return (new List<NewsArticleDto>(), 0);
    }

    public async Task<List<NewsArticleDto>> GetMyHistoryAsync(string token)
    {
        AddAuthHeader(token);
        var response = await _httpClient.GetAsync("api/news/my-history");
        if (response.IsSuccessStatusCode)
        {
            var articles = await response.Content.ReadFromJsonAsync<List<NewsArticleDto>>();
            return articles ?? new List<NewsArticleDto>();
        }
        return new List<NewsArticleDto>();
    }

    public async Task<HttpResponseMessage> CreateNewsArticleAsync(string token, object payload)
    {
        AddAuthHeader(token);
        return await _httpClient.PostAsJsonAsync("api/news", payload);
    }

    public async Task<HttpResponseMessage> UpdateNewsArticleAsync(string token, string id, object payload)
    {
        AddAuthHeader(token);
        return await _httpClient.PutAsJsonAsync($"api/news/{id}", payload);
    }

    public async Task<HttpResponseMessage> DeleteNewsArticleAsync(string token, string id)
    {
        AddAuthHeader(token);
        return await _httpClient.DeleteAsync($"api/news/{id}");
    }

    public async Task<HttpResponseMessage> DuplicateNewsArticleAsync(string token, string id)
    {
        AddAuthHeader(token);
        return await _httpClient.PostAsync($"api/news/{id}/duplicate", null);
    }
}
