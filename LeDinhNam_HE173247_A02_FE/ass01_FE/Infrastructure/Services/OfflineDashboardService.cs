using System.Net.Http;
using System.Threading.Tasks;
using ass01_FE.Infrastructure.Clients;
using ass01_FE.Infrastructure.Helpers;
using ass01_FE.Infrastructure.Models;
using ass01_FE.Presentation.Models.Dashboard;

namespace ass01_FE.Infrastructure.Services;

public class OfflineDashboardService
{
    private readonly AnalyticsApiClient _apiClient;
    private readonly IOfflineCacheService _cacheService;

    public OfflineDashboardService(AnalyticsApiClient apiClient, IOfflineCacheService cacheService)
    {
        _apiClient = apiClient;
        _cacheService = cacheService;
    }

    public async Task<ApiDataResult<DashboardViewModel>> GetDashboardWithOfflineFallbackAsync(DashboardFilterViewModel filter)
    {
        try
        {
            var stats = await _apiClient.GetDashboardStatisticsAsync(filter);
            var trending = await _apiClient.GetTrendingArticlesAsync(filter);

            var dashboard = new DashboardViewModel
            {
                CurrentFilter = filter,
                Statistics = stats,
                TrendingArticles = trending
            };

            await _cacheService.SaveDashboardStatisticsAsync(dashboard);

            return ApiDataResult<DashboardViewModel>.Online(dashboard);
        }
        catch (HttpRequestException ex) when (OfflineHelper.IsApiUnavailableResponse(ex))
        {
            var cached = await _cacheService.GetDashboardStatisticsAsync();
            return ApiDataResult<DashboardViewModel>.Offline(cached.Data, cached.CachedAt);
        }
    }
}
