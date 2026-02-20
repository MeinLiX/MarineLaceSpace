using Basket.WebHost.Data;
using BB.Common.Routes;
using MarineLaceSpace.DTO.Common;
using MarineLaceSpace.DTO.Requests.Basket;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Basket;
using MarineLaceSpace.Interfaces.EventBus;
using MarineLaceSpace.Models.Database.Basket;
using MarineLaceSpace.Models.Events;
using MarineLaceSpace.Models.Routes;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace Basket.WebHost.Routes;

internal class BasketHandlers
{
    private record BasketServices : BasicRouteServices
    {
        public required BasketDbContext DbContext { get; init; }
        public required IHttpContextAccessor HttpContextAccessor { get; init; }
        public required ILogger<BasketHandlers> Logger { get; init; }
        public IEventBus? EventBus { get; init; }
    }

    private static string GetBuyerId(IHttpContextAccessor accessor)
    {
        var userId = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId)) return userId;

        var sessionId = accessor.HttpContext?.Request.Headers["X-Session-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(sessionId)) return sessionId;

        return Guid.NewGuid().ToString();
    }

    private static List<BasketItemResponse> DeserializeItems(string json)
    {
        return JsonSerializer.Deserialize<List<BasketItemResponse>>(json) ?? [];
    }

    private static string SerializeItems(List<BasketItemResponse> items)
    {
        return JsonSerializer.Serialize(items);
    }

    internal static Delegate GetBasketHandler =>
        async (IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<BasketServices>(sp, async (services) =>
            {
                var buyerId = GetBuyerId(services.HttpContextAccessor);
                var basket = await services.DbContext.Baskets.FindAsync(buyerId);

                if (basket == null)
                    return Results.Ok(new BasketResponse { BuyerId = buyerId });

                var items = DeserializeItems(basket.ItemsJson);
                return Results.Ok(new BasketResponse { BuyerId = buyerId, Items = items });
            });

    internal static Delegate AddItemHandler =>
        async (AddBasketItemRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<AddBasketItemRequest, BasketServices>(request, sp,
                async (services) =>
                {
                    var buyerId = GetBuyerId(services.HttpContextAccessor);
                    var basket = await services.DbContext.Baskets.FindAsync(buyerId);

                    List<BasketItemResponse> items;
                    if (basket == null)
                    {
                        basket = new BasketEntity { BuyerId = buyerId };
                        items = [];
                        await services.DbContext.Baskets.AddAsync(basket);
                    }
                    else
                    {
                        items = DeserializeItems(basket.ItemsJson);
                    }

                    var existingItem = items.FirstOrDefault(i =>
                        i.ProductId == request.ProductId &&
                        i.SizeId == request.SizeId &&
                        i.ColorId == request.ColorId &&
                        i.MaterialId == request.MaterialId);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += request.Quantity;
                    }
                    else
                    {
                        items.Add(new BasketItemResponse
                        {
                            ItemId = Guid.NewGuid().ToString(),
                            ProductId = request.ProductId,
                            ProductName = request.ProductName,
                            SizeId = request.SizeId,
                            SizeName = request.SizeName,
                            ColorId = request.ColorId,
                            ColorName = request.ColorName,
                            MaterialId = request.MaterialId,
                            MaterialName = request.MaterialName,
                            UnitPrice = request.UnitPrice,
                            Quantity = request.Quantity,
                            Personalization = request.Personalization,
                            ImageUrl = request.ImageUrl
                        });
                    }

                    basket.ItemsJson = SerializeItems(items);
                    basket.UpdatedAt = DateTime.UtcNow;
                    await services.DbContext.SaveChangesAsync();

                    return Results.Ok(new BasketResponse { BuyerId = buyerId, Items = items });
                });

    internal static Delegate UpdateItemHandler =>
        async (string itemId, UpdateBasketItemRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<UpdateBasketItemRequest, BasketServices>(request, sp,
                async (services) =>
                {
                    var buyerId = GetBuyerId(services.HttpContextAccessor);
                    var basket = await services.DbContext.Baskets.FindAsync(buyerId);
                    if (basket == null) return Results.NotFound(RESTResult.Fail("Basket not found."));

                    var items = DeserializeItems(basket.ItemsJson);
                    var item = items.FirstOrDefault(i => i.ItemId == itemId);
                    if (item == null) return Results.NotFound(RESTResult.Fail("Item not found in basket."));

                    if (request.Quantity.HasValue) item.Quantity = request.Quantity.Value;
                    if (request.Personalization != null) item.Personalization = request.Personalization;

                    if (item.Quantity <= 0) items.Remove(item);

                    basket.ItemsJson = SerializeItems(items);
                    basket.UpdatedAt = DateTime.UtcNow;
                    await services.DbContext.SaveChangesAsync();

                    return Results.Ok(new BasketResponse { BuyerId = buyerId, Items = items });
                });

    internal static Delegate RemoveItemHandler =>
        async (string itemId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<BasketServices>(sp, async (services) =>
            {
                var buyerId = GetBuyerId(services.HttpContextAccessor);
                var basket = await services.DbContext.Baskets.FindAsync(buyerId);
                if (basket == null) return Results.NotFound(RESTResult.Fail("Basket not found."));

                var items = DeserializeItems(basket.ItemsJson);
                var item = items.FirstOrDefault(i => i.ItemId == itemId);
                if (item == null) return Results.NotFound(RESTResult.Fail("Item not found in basket."));

                items.Remove(item);
                basket.ItemsJson = SerializeItems(items);
                basket.UpdatedAt = DateTime.UtcNow;
                await services.DbContext.SaveChangesAsync();

                return Results.Ok(new BasketResponse { BuyerId = buyerId, Items = items });
            });

    internal static Delegate ClearBasketHandler =>
        async (IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<BasketServices>(sp, async (services) =>
            {
                var buyerId = GetBuyerId(services.HttpContextAccessor);
                var deleted = await services.DbContext.Baskets.Where(b => b.BuyerId == buyerId).ExecuteDeleteAsync();
                return Results.NoContent();
            });

    internal static Delegate CheckoutHandler =>
        async (BasketCheckoutRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<BasketCheckoutRequest, BasketServices>(request, sp,
                async (services) =>
                {
                    var httpContext = services.HttpContextAccessor.HttpContext;
                    var buyerId = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(buyerId))
                        return Results.Unauthorized();

                    var basket = await services.DbContext.Baskets.FindAsync(buyerId);
                    if (basket == null) return Results.BadRequest(RESTResult.Fail("Basket is empty."));

                    var items = DeserializeItems(basket.ItemsJson);
                    if (items.Count == 0) return Results.BadRequest(RESTResult.Fail("Basket is empty."));

                    var buyerEmail = httpContext?.User.FindFirstValue(ClaimTypes.Email);

                    if (services.EventBus != null)
                    {
                        await services.EventBus.PublishAsync(new BasketCheckoutEvent
                        {
                            BuyerId = buyerId,
                            BuyerEmail = buyerEmail,
                            TotalPrice = items.Sum(i => i.UnitPrice * i.Quantity),
                            ShippingAddress = request.ShippingAddress,
                            Items = items.Select(i => new BasketCheckoutItem
                            {
                                ProductId = i.ProductId,
                                ProductName = i.ProductName,
                                SizeId = i.SizeId,
                                SizeName = i.SizeName,
                                ColorId = i.ColorId,
                                ColorName = i.ColorName,
                                MaterialId = i.MaterialId,
                                MaterialName = i.MaterialName,
                                UnitPrice = i.UnitPrice,
                                Quantity = i.Quantity,
                                Personalization = i.Personalization,
                                ImageUrl = i.ImageUrl
                            }).ToList()
                        });
                    }

                    services.DbContext.Baskets.Remove(basket);
                    await services.DbContext.SaveChangesAsync();

                    services.Logger.LogInformation("Basket checkout for buyer {BuyerId}", buyerId);
                    return Results.Ok(RESTResult.Success("Checkout initiated. Order will be created."));
                });
}
