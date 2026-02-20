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
                ImageUrl = i.ImageUrl
            }).ToList()
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
                            ImageUrl = request.ImageUrl
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
                            ImageUrl = i.ImageUrl
                        }).ToList()
                    });

                    await services.BasketRepository.DeleteBasketAsync(buyerId);

                    return Results.Ok(RESTResult.Success("Checkout initiated. Order will be created."));
                });
}
