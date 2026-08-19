using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ass01_FE.DataAccess.Services;
using ass01_FE.Infrastructure.Helpers;
using ass01_FE.Infrastructure.Models;
using ass01_FE.Presentation.Models.News;

namespace ass01_FE.Infrastructure.Services;

public class OfflineTagService
{
    private readonly TagApiService _apiService;
    private readonly IOfflineCacheService _cacheService;

    public OfflineTagService(TagApiService apiService, IOfflineCacheService cacheService)
    {
        _apiService = apiService;
        _cacheService = cacheService;
    }

    public async Task<ApiDataResult<List<TagDto>>> GetTagsWithOfflineFallbackAsync()
    {
        try
        {
            var tags = await _apiService.GetTagsAsync();
            if (tags != null)
            {
                await _cacheService.SaveTagsAsync(tags);
            }
            return ApiDataResult<List<TagDto>>.Online(tags ?? new List<TagDto>());
        }
        catch (HttpRequestException ex) when (OfflineHelper.IsApiUnavailableResponse(ex))
        {
            var cached = await _cacheService.GetTagsAsync();
            return ApiDataResult<List<TagDto>>.Offline(cached.Data, cached.CachedAt);
        }
    }
}
