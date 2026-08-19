using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ass01_FE.Presentation.Models.Account;

namespace ass01_FE.DataAccess.Services
{
    public class ProfileApiService
    {
        private readonly HttpClient _httpClient;

        public ProfileApiService(HttpClient httpClient, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _httpClient = httpClient;
            var baseUrl = configuration["ApiSettings:CoreApi"] ?? throw new System.InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            _httpClient.BaseAddress = new System.Uri(baseUrl);
        }



        public async Task<AccountDto?> GetMyProfileAsync()
        {
            return await _httpClient.GetFromJsonAsync<AccountDto>("/api/profile");
        }

        public async Task<HttpResponseMessage> UpdateMyProfileAsync(object payload)
        {
            return await _httpClient.PutAsJsonAsync("/api/profile", payload);
        }

        public async Task<HttpResponseMessage> ChangePasswordAsync(short id, object payload)
        {
            return await _httpClient.PostAsJsonAsync($"/api/account/{id}/change-password", payload);
        }
    }
}
