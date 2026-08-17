using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ass01_FE.Presentation.Models.News;

namespace ass01_FE.DataAccess.Services
{
    public class TagApiService
    {
        private readonly HttpClient _httpClient;

        public TagApiService(HttpClient httpClient, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _httpClient = httpClient;
            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? throw new System.InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            _httpClient.BaseAddress = new System.Uri(baseUrl);
        }

        private void AddAuthHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<TagDto>?> GetTagsAsync(string token)
        {
            AddAuthHeader(token);
            return await _httpClient.GetFromJsonAsync<List<TagDto>>("/api/tag");
        }

        public async Task<TagDto?> GetTagByIdAsync(string token, int id)
        {
            AddAuthHeader(token);
            return await _httpClient.GetFromJsonAsync<TagDto>($"/api/tag/{id}");
        }

        public async Task<HttpResponseMessage> CreateTagAsync(string token, object payload)
        {
            AddAuthHeader(token);
            return await _httpClient.PostAsJsonAsync("/api/tag", payload);
        }

        public async Task<HttpResponseMessage> UpdateTagAsync(string token, int id, object payload)
        {
            AddAuthHeader(token);
            return await _httpClient.PutAsJsonAsync($"/api/tag/{id}", payload);
        }

        public async Task<HttpResponseMessage> DeleteTagAsync(string token, int id)
        {
            AddAuthHeader(token);
            return await _httpClient.DeleteAsync($"/api/tag/{id}");
        }

        public async Task<List<object>?> GetNewsForTagAsync(string token, int id)
        {
            AddAuthHeader(token);
            return await _httpClient.GetFromJsonAsync<List<object>>($"/api/tag/{id}/news-articles");
        }
    }
}
