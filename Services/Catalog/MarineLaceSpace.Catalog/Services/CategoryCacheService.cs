using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Catalog.WebHost.Services;

public interface ICategoryCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task InvalidateAsync(string key);
    Task InvalidateCategoryTreeAsync();
}

public class CategoryCacheService : ICategoryCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CategoryCacheService> _logger;
    private const string CategoryTreeKey = "categories:tree";
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public CategoryCacheService(IDistributedCache cache, ILogger<CategoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var data = await _cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(data)) return default;
        _logger.LogDebug("Cache hit for key {Key}", key);
        return JsonSerializer.Deserialize<T>(data);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
        };
        var json = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, json, options);
        _logger.LogDebug("Cache set for key {Key}", key);
    }

    public async Task InvalidateAsync(string key)
    {
        await _cache.RemoveAsync(key);
        _logger.LogDebug("Cache invalidated for key {Key}", key);
    }

    public async Task InvalidateCategoryTreeAsync()
    {
        await _cache.RemoveAsync(CategoryTreeKey);
        _logger.LogInformation("Category tree cache invalidated");
    }
}
