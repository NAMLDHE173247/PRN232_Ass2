using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ass01_FE.Presentation.Models.Account;
using ass01_FE.Presentation.Models.News; // For ODataResponse

namespace ass01_FE.DataAccess.Services;

public class AccountApiService
{
    private readonly HttpClient _httpClient;

    public AccountApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var baseUrl = configuration["ApiSettings:CoreApi"] ?? throw new System.InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        _httpClient.BaseAddress = new System.Uri(baseUrl);
    }



    public async Task<List<AccountDto>> GetAccountsAsync(string? keyword = null, short? role = null)
    {
        
        var url = "api/account?";
        if (!string.IsNullOrEmpty(keyword)) url += $"searchKeyword={System.Uri.EscapeDataString(keyword)}&";
        if (role.HasValue) url += $"roleFilter={role}&";

        var response = await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            // OData might wrap the response
            var contentString = await response.Content.ReadAsStringAsync();
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var odataResult = System.Text.Json.JsonSerializer.Deserialize<ODataResponse<AccountDto>>(contentString, options);
                if (odataResult != null && odataResult.Value != null)
                {
                    return odataResult.Value;
                }
            }
            catch
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var listResult = System.Text.Json.JsonSerializer.Deserialize<List<AccountDto>>(contentString, options);
                if (listResult != null) return listResult;
            }
        }
        return new List<AccountDto>();
    }

    public async Task<(bool Success, string Message)> CreateAccountAsync(CreateAccountViewModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/account", model);
        if (response.IsSuccessStatusCode)
        {
            return (true, "Account created successfully.");
        }
        var errorMsg = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrEmpty(errorMsg) ? "Failed to create account." : errorMsg);
    }

    public async Task<(bool Success, string Message)> UpdateAccountAsync(UpdateAccountViewModel model)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/account/{model.AccountId}", model);
        if (response.IsSuccessStatusCode)
        {
            return (true, "Account updated successfully.");
        }
        var errorMsg = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrEmpty(errorMsg) ? "Failed to update account." : errorMsg);
    }

    public async Task<(bool Success, string Message)> DeleteAccountAsync(short id)
    {
        var response = await _httpClient.DeleteAsync($"api/account/{id}");
        if (response.IsSuccessStatusCode)
        {
            return (true, "Account deleted successfully.");
        }
        var errorMsg = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrEmpty(errorMsg) ? "Failed to delete account." : errorMsg);
    }
}
