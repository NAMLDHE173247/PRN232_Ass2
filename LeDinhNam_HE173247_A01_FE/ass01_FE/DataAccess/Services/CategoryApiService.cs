using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ass01_FE.DataAccess.Services
{
    public class CategoryApiService
    {
        private readonly HttpClient _httpClient;

        public CategoryApiService(HttpClient httpClient, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _httpClient = httpClient;
            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? throw new System.InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            _httpClient.BaseAddress = new System.Uri(baseUrl);
        }

        private void AddAuthHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<CategoryListResult?> GetCategoriesAsync(string token, string? search = null, int? skip = null, int? top = null)
        {
            AddAuthHeader(token);
            var query = new List<string>();
            if (!string.IsNullOrEmpty(search)) query.Add($"search={search}");
            if (skip.HasValue) query.Add($"skip={skip}");
            if (top.HasValue) query.Add($"top={top}");

            var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
            return await _httpClient.GetFromJsonAsync<CategoryListResult>($"/api/category{qs}");
        }

        public async Task<object?> GetCategoryByIdAsync(string token, short id)
        {
            AddAuthHeader(token);
            return await _httpClient.GetFromJsonAsync<object>($"/api/category/{id}");
        }

        public async Task<HttpResponseMessage> CreateCategoryAsync(string token, object payload)
        {
            AddAuthHeader(token);
            return await _httpClient.PostAsJsonAsync("/api/category", payload);
        }

        public async Task<HttpResponseMessage> UpdateCategoryAsync(string token, short id, object payload)
        {
            AddAuthHeader(token);
            return await _httpClient.PutAsJsonAsync($"/api/category/{id}", payload);
        }

        public async Task<HttpResponseMessage> DeleteCategoryAsync(string token, short id)
        {
            AddAuthHeader(token);
            return await _httpClient.DeleteAsync($"/api/category/{id}");
        }
    }

    public class CategoryListResult
    {
        public List<CategoryDto> Value { get; set; } = new();
        public int Count { get; set; }
    }

    public class CategoryDto
    {
        public short CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryDescription { get; set; }
        public short? ParentCategoryId { get; set; }
        public bool? IsActive { get; set; }
        public int ArticleCount { get; set; }
    }
}
