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
            var baseUrl = configuration["ApiSettings:CoreApi"] ?? throw new System.InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            _httpClient.BaseAddress = new System.Uri(baseUrl);
        }



        public async Task<CategoryListResult?> GetCategoriesAsync(string? search = null, int? skip = null, int? top = null)
        {
            var query = new List<string>();
            if (!string.IsNullOrEmpty(search)) query.Add($"search={search}");
            if (skip.HasValue) query.Add($"skip={skip}");
            if (top.HasValue) query.Add($"top={top}");

            var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
            return await _httpClient.GetFromJsonAsync<CategoryListResult>($"/api/category{qs}");
        }

        public async Task<object?> GetCategoryByIdAsync(short id)
        {
            return await _httpClient.GetFromJsonAsync<object>($"/api/category/{id}");
        }

        public async Task<HttpResponseMessage> CreateCategoryAsync(object payload)
        {
            return await _httpClient.PostAsJsonAsync("/api/category", payload);
        }

        public async Task<HttpResponseMessage> UpdateCategoryAsync(short id, object payload)
        {
            return await _httpClient.PutAsJsonAsync($"/api/category/{id}", payload);
        }

        public async Task<HttpResponseMessage> DeleteCategoryAsync(short id)
        {
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
