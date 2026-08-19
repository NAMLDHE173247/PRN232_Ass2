using System;

namespace ass01_FE.Infrastructure.Models;

public class ApiDataResult<T>
{
    public T? Data { get; set; }
    public bool IsOffline { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset? CachedAt { get; set; }

    public static ApiDataResult<T> Online(T data)
    {
        return new ApiDataResult<T>
        {
            Data = data,
            IsOffline = false
        };
    }

    public static ApiDataResult<T> Offline(T? data, DateTimeOffset? cachedAt = null, string? errorMessage = null)
    {
        return new ApiDataResult<T>
        {
            Data = data,
            IsOffline = true,
            CachedAt = cachedAt,
            ErrorMessage = errorMessage ?? "The API is currently unavailable. Showing cached data."
        };
    }
}
