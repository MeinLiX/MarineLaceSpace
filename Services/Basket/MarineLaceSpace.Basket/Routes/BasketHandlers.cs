using Basket.WebHost.Data;
using BB.Common.Routes;
using MarineLaceSpace.DTO.Requests.Basket;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Basket;
using MarineLaceSpace.Interfaces.EventBus;
using MarineLaceSpace.Models.Events;
using MarineLaceSpace.Models.Routes;
using System.Security.Claims;

namespace Basket.WebHost.Routes;

internal class BasketHandlers
{
    private record BasketServices : BasicRouteServices
    {
        public required IBasketRepository BasketRepository { get; init; }
        public required IHttpContextAccessor HttpContextAccessor { get; init; }
        public required IEventBus EventBus { get; init; }
    }

    private static string GetBuyerId(IHttpContextAccessor accessor)
    {
        var userId = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId)) return userId;

        var sessionId = accessor.HttpContext?.Request.Headers["X-Session-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(sessionId)) return sessionId;

        return Guid.NewGuid().ToString();
    }

    private static BasketResponse ToResponse(string buyerId, BasketData? data)
    {
        if (data == null)
            return new BasketResponse { BuyerId = buyerId };

        return new BasketResponse
        {
            BuyerId = buyerId,
            Items = data.Items.Select(i => new BasketItemResponse
            {
                ItemId = i.ItemId,
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
                ImageUrl = i.ImageUrl,
                ShopId = i.ShopId
            }).ToList(),
            TotalPrice = data.Items.Sum(i => i.UnitPrice * i.Quantity),
            TotalItems = data.Items.Sum(i => i.Quantity)
        };
    }

    internal static Delegate GetBasketHandler =>
        async (IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<BasketServices>(sp, async (services) =>
            {
                var buyerId = GetBuyerId(services.HttpContextAccessor);
                var basket = await services.BasketRepository.GetBasketAsync(buyerId);
                return Results.Ok(ToResponse(buyerId, basket));
            });

    internal static Delegate AddItemHandler =>
        async (AddBasketItemRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<AddBasketItemRequest, BasketServices>(request, sp,
                async (services) =>
                {
                    var buyerId = GetBuyerId(services.HttpContextAccessor);
                    var basket = await services.BasketRepository.GetBasketAsync(buyerId)
                                 ?? new BasketData { BuyerId = buyerId };

                    var existingItem = basket.Items.FirstOrDefault(i =>
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
                        basket.Items.Add(new BasketItemData
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
                            ImageUrl = request.ImageUrl,
                            ShopId = request.ShopId
                        });
                    }

                    var updated = await services.BasketRepository.UpdateBasketAsync(buyerId, basket);
                    return Results.Ok(ToResponse(buyerId, updated));
                });

    internal static Delegate UpdateItemHandler =>
        async (string itemId, UpdateBasketItemRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<UpdateBasketItemRequest, BasketServices>(request, sp,
                async (services) =>
                {
                    var buyerId = GetBuyerId(services.HttpContextAccessor);
                    var basket = await services.BasketRepository.GetBasketAsync(buyerId);
                    if (basket == null) return Results.NotFound(RESTResult.Fail("Basket not found."));

                    var item = basket.Items.FirstOrDefault(i => i.ItemId == itemId);
                    if (item == null) return Results.NotFound(RESTResult.Fail("Item not found in basket."));

                    if (request.Quantity.HasValue) item.Quantity = request.Quantity.Value;
                    if (request.Personalization != null) item.Personalization = request.Personalization;

                    if (item.Quantity <= 0) basket.Items.Remove(item);

                    var updated = await services.BasketRepository.UpdateBasketAsync(buyerId, basket);
                    return Results.Ok(ToResponse(buyerId, updated));
                });

    internal static Delegate RemoveItemHandler =>
        async (string itemId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<BasketServices>(sp, async (services) =>
            {
                var buyerId = GetBuyerId(services.HttpContextAccessor);
                var basket = await services.BasketRepository.GetBasketAsync(buyerId);
                if (basket == null) return Results.NotFound(RESTResult.Fail("Basket not found."));

                var item = basket.Items.FirstOrDefault(i => i.ItemId == itemId);
                if (item == null) return Results.NotFound(RESTResult.Fail("Item not found in basket."));

                basket.Items.Remove(item);
                await services.BasketRepository.UpdateBasketAsync(buyerId, basket);

                return Results.Ok(ToResponse(buyerId, basket));
            });

    internal static Delegate ClearBasketHandler =>
        async (IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<BasketServices>(sp, async (services) =>
            {
                var buyerId = GetBuyerId(services.HttpContextAccessor);
                await services.BasketRepository.DeleteBasketAsync(buyerId);
                return Results.NoContent();
            });

    internal static Delegate GetBasketCountHandler =>
        async (IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<BasketServices>(sp, async (services) =>
            {
                var buyerId = GetBuyerId(services.HttpContextAccessor);
                var basket = await services.BasketRepository.GetBasketAsync(buyerId);
                var itemCount = basket?.Items.Sum(i => i.Quantity) ?? 0;
                var uniqueItems = basket?.Items.Count ?? 0;
                return Results.Ok(new { ItemCount = itemCount, UniqueItems = uniqueItems });
            });

    internal static Delegate GetBasketTotalHandler =>
        async (IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<BasketServices>(sp, async (services) =>
            {
                var buyerId = GetBuyerId(services.HttpContextAccessor);
                var basket = await services.BasketRepository.GetBasketAsync(buyerId);
                if (basket == null || basket.Items.Count == 0)
                    return Results.Ok(new { Total = 0m, ItemCount = 0, Currency = "UAH" });

                var total = basket.Items.Sum(i => i.UnitPrice * i.Quantity);
                var itemCount = basket.Items.Sum(i => i.Quantity);
                return Results.Ok(new { Total = total, ItemCount = itemCount, Currency = "UAH" });
            });

    internal static Delegate MergeBasketHandler =>
        async (MergeBasketRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<MergeBasketRequest, BasketServices>(request, sp,
                async (services) =>
                {
                    var httpContext = services.HttpContextAccessor.HttpContext;
                    var userId = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                    var anonymousBasketId = $"anon-{request.SessionId}";
                    var anonymousBasket = await services.BasketRepository.GetBasketAsync(anonymousBasketId);
                    if (anonymousBasket == null || anonymousBasket.Items.Count == 0)
                        return Results.Ok(RESTResult.Success("No anonymous basket to merge."));

                    var userBasket = await services.BasketRepository.GetBasketAsync(userId)
                                     ?? new BasketData { BuyerId = userId };

                    foreach (var anonItem in anonymousBasket.Items)
                    {
                        var existingItem = userBasket.Items.FirstOrDefault(i =>
                            i.ProductId == anonItem.ProductId &&
                            i.SizeId == anonItem.SizeId &&
                            i.ColorId == anonItem.ColorId &&
                            i.MaterialId == anonItem.MaterialId);

                        if (existingItem != null)
                        {
                            existingItem.Quantity += anonItem.Quantity;
                        }
                        else
                        {
                            anonItem.ItemId = Guid.NewGuid().ToString();
                            userBasket.Items.Add(anonItem);
                        }
                    }

                    await services.BasketRepository.UpdateBasketAsync(userId, userBasket);
                    await services.BasketRepository.DeleteBasketAsync(anonymousBasketId);

                    return Results.Ok(ToResponse(userId, userBasket));
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

                    var basket = await services.BasketRepository.GetBasketAsync(buyerId);
                    if (basket == null || basket.Items.Count == 0)
                        return Results.BadRequest(RESTResult.Fail("Basket is empty."));

                    var buyerEmail = httpContext?.User.FindFirstValue(ClaimTypes.Email);

                    await services.EventBus.PublishAsync(new BasketCheckoutEvent
                    {
                        BuyerId = buyerId,
                        BuyerEmail = buyerEmail,
                        TotalPrice = basket.Items.Sum(i => i.UnitPrice * i.Quantity),
                        ShippingAddress = request.ShippingAddress,
                        Items = basket.Items.Select(i => new BasketCheckoutItem
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
                            ImageUrl = i.ImageUrl,
                            ShopId = i.ShopId
                        }).ToList()
                    });

                    await services.BasketRepository.DeleteBasketAsync(buyerId);

                    return Results.Ok(RESTResult.Success("Checkout initiated. Order will be created."));
                });
}
