namespace ass01_FE.DataAccess.Services;

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ass01_FE.Presentation.Models.Auth;

public class AuthApiService
{
    private readonly HttpClient _httpClient;

    public AuthApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? throw new System.InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        _httpClient.BaseAddress = new System.Uri(baseUrl);
    }

    public async Task<LoginResponse?> LoginAsync(LoginViewModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", model);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        return null;
    }
}
