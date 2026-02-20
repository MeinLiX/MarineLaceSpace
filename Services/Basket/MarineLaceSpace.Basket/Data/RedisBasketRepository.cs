using MarineLaceSpace.DTO.Responses.Basket;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Basket.WebHost.Data;

public interface IBasketRepository
{
    Task<BasketData?> GetBasketAsync(string buyerId);
    Task<BasketData> UpdateBasketAsync(string buyerId, BasketData basket);
    Task DeleteBasketAsync(string buyerId);
}

public class BasketData
{
    public string BuyerId { get; set; } = string.Empty;
    public List<BasketItemData> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class BasketItemData
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString();
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? SizeId { get; set; }
    public string? SizeName { get; set; }
    public string? ColorId { get; set; }
    public string? ColorName { get; set; }
    public string? MaterialId { get; set; }
    public string? MaterialName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? Personalization { get; set; }
    public string? ImageUrl { get; set; }
}

public class RedisBasketRepository : IBasketRepository
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisBasketRepository> _logger;
    private static readonly TimeSpan AnonymousSessionTtl = TimeSpan.FromDays(30);
    private static readonly TimeSpan AuthenticatedSessionTtl = TimeSpan.FromDays(90);

    public RedisBasketRepository(IDistributedCache cache, ILogger<RedisBasketRepository> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    private static string GetKey(string buyerId) => $"basket:{buyerId}";

    public async Task<BasketData?> GetBasketAsync(string buyerId)
    {
        var data = await _cache.GetStringAsync(GetKey(buyerId));
        if (string.IsNullOrEmpty(data))
            return null;

        return JsonSerializer.Deserialize<BasketData>(data);
    }

    public async Task<BasketData> UpdateBasketAsync(string buyerId, BasketData basket)
    {
        basket.BuyerId = buyerId;
        basket.UpdatedAt = DateTime.UtcNow;

        var isAnonymous = buyerId.StartsWith("anon-");
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = isAnonymous ? AnonymousSessionTtl : AuthenticatedSessionTtl
        };

        var json = JsonSerializer.Serialize(basket);
        await _cache.SetStringAsync(GetKey(buyerId), json, options);

        _logger.LogInformation("Basket updated for buyer {BuyerId}, {ItemCount} items", buyerId, basket.Items.Count);
        return basket;
    }

    public async Task DeleteBasketAsync(string buyerId)
    {
        await _cache.RemoveAsync(GetKey(buyerId));
        _logger.LogInformation("Basket deleted for buyer {BuyerId}", buyerId);
    }
}
