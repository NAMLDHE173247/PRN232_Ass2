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
            var baseUrl = configuration["ApiSettings:CoreApi"] ?? throw new System.InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            _httpClient.BaseAddress = new System.Uri(baseUrl);
        }



        public async Task<List<TagDto>?> GetTagsAsync()
        {
            var response = await _httpClient.GetAsync("/api/tag");
            ass01_FE.Infrastructure.Helpers.OfflineHelper.ThrowIfOffline(response);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<TagDto>>();
            }
            return null;
        }

        public async Task<TagDto?> GetTagByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<TagDto>($"/api/tag/{id}");
        }

        public async Task<HttpResponseMessage> CreateTagAsync(object payload)
        {
            return await _httpClient.PostAsJsonAsync("/api/tag", payload);
        }

        public async Task<HttpResponseMessage> UpdateTagAsync(int id, object payload)
        {
            return await _httpClient.PutAsJsonAsync($"/api/tag/{id}", payload);
        }

        public async Task<HttpResponseMessage> DeleteTagAsync(int id)
        {
            return await _httpClient.DeleteAsync($"/api/tag/{id}");
        }

        public async Task<List<object>?> GetNewsForTagAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<List<object>>($"/api/tag/{id}/news-articles");
        }
    }
}
