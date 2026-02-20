using BB.Common.Routes;
using MarineLaceSpace.DTO.Requests.Order;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Order;
using MarineLaceSpace.Enumerations;
using MarineLaceSpace.Interfaces.EventBus;
using MarineLaceSpace.Models.Events;
using MarineLaceSpace.Models.Routes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.WebHost.Data;
using System.Security.Claims;

namespace Order.WebHost.Routes;

internal class OrderHandlers
{
    private record OrderServices : BasicRouteServices
    {
        public required OrderDbContext DbContext { get; init; }
        public required IHttpContextAccessor HttpContextAccessor { get; init; }
        public required ILogger<OrderHandlers> Logger { get; init; }
        public IEventBus? EventBus { get; init; }
    }

    internal static Delegate GetMyOrdersHandler =>
        async (IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<OrderServices>(sp, async (services) =>
            {
                var userId = services.HttpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var orders = await services.DbContext.Orders
                    .Where(o => o.BuyerId == userId)
                    .Include(o => o.Items)
                    .OrderByDescending(o => o.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(orders.Select(MapOrderToResponse));
            });

    internal static Delegate GetOrderByIdHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<OrderServices>(sp, async (services) =>
            {
                var userId = services.HttpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = services.HttpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;

                var order = await services.DbContext.Orders
                    .Include(o => o.Items)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null) return Results.NotFound(RESTResult.Fail("Order not found."));
                if (!isAdmin && order.BuyerId != userId) return Results.Forbid();

                return Results.Ok(MapOrderToResponse(order));
            });

    internal static Delegate GetShopOrdersHandler =>
        async (string shopId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<OrderServices>(sp, async (services) =>
            {
                var orders = await services.DbContext.Orders
                    .Include(o => o.Items)
                    .OrderByDescending(o => o.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(orders.Select(MapOrderToResponse));
            });

    internal static Delegate UpdateOrderStatusHandler =>
        async (string id, [FromBody] UpdateOrderStatusRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<UpdateOrderStatusRequest, OrderServices>(request, sp,
                async (services) =>
                {
                    var order = await services.DbContext.Orders.FindAsync(id);
                    if (order == null) return Results.NotFound(RESTResult.Fail("Order not found."));

                    var oldStatus = OrderStatus.FromId<OrderStatus>(order.StatusId)?.Name ?? "Unknown";
                    var newStatus = OrderStatus.FromId<OrderStatus>(request.StatusId);
                    if (newStatus == null) return Results.BadRequest(RESTResult.Fail("Invalid status ID."));

                    order.StatusId = request.StatusId;
                    order.UpdatedAt = DateTime.UtcNow;
                    await services.DbContext.SaveChangesAsync();

                    if (services.EventBus != null)
                    {
                        await services.EventBus.PublishAsync(new OrderStatusChangedEvent
                        {
                            OrderId = order.Id,
                            BuyerId = order.BuyerId,
                            BuyerEmail = order.BuyerEmail,
                            OldStatus = oldStatus,
                            NewStatus = newStatus.Name
                        });
                    }

                    services.Logger.LogInformation("Order {OrderId} status changed from {OldStatus} to {NewStatus}", id, oldStatus, newStatus.Name);
                    return Results.Ok(MapOrderToResponse(order));
                });

    internal static Delegate AddTrackingHandler =>
        async (string id, [FromBody] AddTrackingRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<AddTrackingRequest, OrderServices>(request, sp,
                async (services) =>
                {
                    var order = await services.DbContext.Orders.FindAsync(id);
                    if (order == null) return Results.NotFound(RESTResult.Fail("Order not found."));

                    order.TrackingNumber = request.TrackingNumber;
                    order.StatusId = OrderStatus.Shipped.Id;
                    order.UpdatedAt = DateTime.UtcNow;
                    await services.DbContext.SaveChangesAsync();

                    if (services.EventBus != null)
                    {
                        await services.EventBus.PublishAsync(new OrderStatusChangedEvent
                        {
                            OrderId = order.Id,
                            BuyerId = order.BuyerId,
                            BuyerEmail = order.BuyerEmail,
                            OldStatus = "Processing",
                            NewStatus = OrderStatus.Shipped.Name
                        });
                    }

                    return Results.Ok(MapOrderToResponse(order));
                });

    internal static Delegate CancelOrderHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<OrderServices>(sp, async (services) =>
            {
                var userId = services.HttpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = services.HttpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;

                var order = await services.DbContext.Orders.FindAsync(id);
                if (order == null) return Results.NotFound(RESTResult.Fail("Order not found."));
                if (!isAdmin && order.BuyerId != userId) return Results.Forbid();

                var currentStatus = OrderStatus.FromId<OrderStatus>(order.StatusId);
                if (currentStatus == OrderStatus.Shipped || currentStatus == OrderStatus.Delivered || currentStatus == OrderStatus.Completed)
                    return Results.BadRequest(RESTResult.Fail("Cannot cancel order in current status."));

                var oldStatus = currentStatus?.Name ?? "Unknown";
                order.StatusId = OrderStatus.Canceled.Id;
                order.UpdatedAt = DateTime.UtcNow;
                await services.DbContext.SaveChangesAsync();

                if (services.EventBus != null)
                {
                    await services.EventBus.PublishAsync(new OrderStatusChangedEvent
                    {
                        OrderId = order.Id,
                        BuyerId = order.BuyerId,
                        BuyerEmail = order.BuyerEmail,
                        OldStatus = oldStatus,
                        NewStatus = OrderStatus.Canceled.Name
                    });
                }

                return Results.Ok(MapOrderToResponse(order));
            });

    private static OrderResponse MapOrderToResponse(MarineLaceSpace.Models.Database.Order.Order order) => new()
    {
        Id = order.Id,
        BuyerId = order.BuyerId,
        BuyerEmail = order.BuyerEmail,
        Status = OrderStatus.FromId<OrderStatus>(order.StatusId)?.Name ?? "Unknown",
        TotalPrice = order.TotalPrice,
        TrackingNumber = order.TrackingNumber,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt,
        ShippingAddress = new ShippingAddressInfo
        {
            FullName = order.ShippingFullName,
            AddressLine1 = order.ShippingAddressLine1,
            AddressLine2 = order.ShippingAddressLine2,
            City = order.ShippingCity,
            PostalCode = order.ShippingPostalCode,
            Country = order.ShippingCountry,
            PhoneNumber = order.ShippingPhoneNumber
        },
        Items = order.Items?.Select(i => new OrderItemResponse
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            SizeName = i.SizeName,
            ColorName = i.ColorName,
            MaterialName = i.MaterialName,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            Personalization = i.Personalization,
            ImageUrl = i.ImageUrl
        }).ToList() ?? []
    };
}
