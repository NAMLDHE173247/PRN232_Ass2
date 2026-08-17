using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ass01_FE.Presentation.Models.Report;

namespace ass01_FE.DataAccess.Services;

public class ReportApiService
{
    private readonly HttpClient _httpClient;

    public ReportApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? throw new System.InvalidOperationException("ApiSettings:BaseUrl is not configured.");
        _httpClient.BaseAddress = new System.Uri(baseUrl);
    }

    public async Task<ReportStatisticsDto?> GetReportAsync(string token, DateTime startDate, DateTime endDate)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var url = $"api/report?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ReportStatisticsDto>();
        }
        
        return null;
    }
}
