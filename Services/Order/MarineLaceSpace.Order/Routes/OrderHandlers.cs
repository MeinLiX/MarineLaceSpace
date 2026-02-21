using BB.Common.Routes;
using MarineLaceSpace.DTO.Requests.Order;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Order;
using MarineLaceSpace.Enumerations;
using MarineLaceSpace.Interfaces.EventBus;
using MarineLaceSpace.Models.Events;
using MarineLaceSpace.Models.Routes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.WebHost.Data;
using System.Net.Http.Json;
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
        public required IHttpClientFactory HttpClientFactory { get; init; }
    }

    /// <summary>
    /// Verifies that the current seller owns the given shop by calling Catalog service.
    /// Returns true if the user is admin or owns the shop.
    /// </summary>
    private static async Task<bool> VerifySellerOwnsShopAsync(OrderServices services, string shopId)
    {
        var httpContext = services.HttpContextAccessor.HttpContext!;
        var isAdmin = httpContext.User.IsInRole("Admin");
        if (isAdmin) return true;

        var currentUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId)) return false;

        try
        {
            var client = services.HttpClientFactory.CreateClient("catalog-api");
            // Forward the authorization header so the catalog service can verify
            var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);

            var response = await client.GetAsync($"/shops/{shopId}");
            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadFromJsonAsync<ShopOwnerInfo>();
            return json?.OwnerId == currentUserId;
        }
        catch (Exception ex)
        {
            services.Logger.LogError(ex, "Failed to verify shop ownership for shop {ShopId}", shopId);
            return false;
        }
    }

    private record ShopOwnerInfo
    {
        public string? OwnerId { get; init; }
    }

    internal static Delegate GetMyOrdersHandler =>
        async ([AsParameters] OrderFilterRequest filter, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<OrderServices>(sp, async (services) =>
            {
                var userId = services.HttpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var query = services.DbContext.Orders
                    .Where(o => o.BuyerId == userId)
                    .AsNoTracking();

                query = ApplyFilters(query, filter);
                var totalCount = await query.CountAsync();
                query = ApplySorting(query, filter.SortBy, filter.SortDesc ?? true);

                var page = Math.Max(1, filter.Page ?? 1);
                var pageSize = Math.Clamp(filter.PageSize ?? 20, 1, 100);

                var orders = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(o => o.Items)
                    .ToListAsync();

                var response = new { TotalCount = totalCount, Page = page, PageSize = pageSize, Items = orders.Select(MapOrderToResponse) };
                return Results.Ok(response);
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

                if (!isAdmin && order.BuyerId != userId)
                {
                    var isSeller = services.HttpContextAccessor.HttpContext?.User.IsInRole("Seller") ?? false;
                    if (!isSeller || !await VerifySellerOwnsShopAsync(services, order.ShopId))
                        return Results.Forbid();
                }

                return Results.Ok(MapOrderToResponse(order));
            });

    internal static Delegate GetShopOrdersHandler =>
        async (string shopId, [AsParameters] OrderFilterRequest filter, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<OrderServices>(sp, async (services) =>
            {
                if (!await VerifySellerOwnsShopAsync(services, shopId))
                    return Results.Forbid();

                var query = services.DbContext.Orders
                    .Where(o => o.ShopId == shopId)
                    .AsNoTracking();

                query = ApplyFilters(query, filter);
                var totalCount = await query.CountAsync();
                query = ApplySorting(query, filter.SortBy, filter.SortDesc ?? true);

                var page = Math.Max(1, filter.Page ?? 1);
                var pageSize = Math.Clamp(filter.PageSize ?? 20, 1, 100);

                var orders = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(o => o.Items)
                    .ToListAsync();

                var response = new { TotalCount = totalCount, Page = page, PageSize = pageSize, Items = orders.Select(MapOrderToResponse) };
                return Results.Ok(response);
            });

    internal static Delegate GetShopOrderStatisticsHandler =>
        async (string shopId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<OrderServices>(sp, async (services) =>
            {
                if (!await VerifySellerOwnsShopAsync(services, shopId))
                    return Results.Forbid();

                var orders = await services.DbContext.Orders
                    .Where(o => o.ShopId == shopId)
                    .AsNoTracking()
                    .ToListAsync();

                var totalOrders = orders.Count;
                var totalRevenue = orders.Sum(o => o.TotalPrice);
                var averageOrderValue = totalOrders > 0 ? Math.Round(totalRevenue / totalOrders, 2) : 0m;

                var ordersByStatus = orders
                    .GroupBy(o => o.StatusId)
                    .Select(g => new
                    {
                        Status = OrderStatus.FromId<OrderStatus>(g.Key)?.Name ?? "Unknown",
                        Count = g.Count(),
                        Revenue = g.Sum(o => o.TotalPrice)
                    })
                    .OrderBy(s => s.Status)
                    .ToList();

                var response = new
                {
                    TotalOrders = totalOrders,
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = averageOrderValue,
                    OrdersByStatus = ordersByStatus
                };

                return Results.Ok(response);
            });

    internal static Delegate UpdateOrderStatusHandler =>
        async (string id, [FromBody] UpdateOrderStatusRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<UpdateOrderStatusRequest, OrderServices>(request, sp,
                async (services) =>
                {
                    var order = await services.DbContext.Orders.FindAsync(id);
                    if (order == null) return Results.NotFound(RESTResult.Fail("Order not found."));

                    if (!await VerifySellerOwnsShopAsync(services, order.ShopId))
                        return Results.Forbid();

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

                    if (!await VerifySellerOwnsShopAsync(services, order.ShopId))
                        return Results.Forbid();

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

                if (currentStatus != OrderStatus.New
                    && (currentStatus == OrderStatus.Paid || currentStatus == OrderStatus.Processing)
                    && (DateTime.UtcNow - order.CreatedAt).TotalHours > 24)
                    return Results.BadRequest(RESTResult.Fail("Cancellation window has expired. Please contact support."));

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

    internal static Delegate GetAllOrdersAdminHandler =>
        async ([AsParameters] OrderFilterRequest filter, string? search, string? shopId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<OrderServices>(sp, async (services) =>
            {
                var query = services.DbContext.Orders.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(shopId))
                    query = query.Where(o => o.ShopId == shopId);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(o =>
                        o.Id.ToLower().Contains(searchLower) ||
                        (o.BuyerEmail != null && o.BuyerEmail.ToLower().Contains(searchLower)) ||
                        o.ShippingFullName.ToLower().Contains(searchLower));
                }

                query = ApplyFilters(query, filter);
                var totalCount = await query.CountAsync();
                query = ApplySorting(query, filter.SortBy, filter.SortDesc ?? true);

                var page = Math.Max(1, filter.Page ?? 1);
                var pageSize = Math.Clamp(filter.PageSize ?? 20, 1, 100);

                var orders = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(o => o.Items)
                    .ToListAsync();

                var response = new { TotalCount = totalCount, Page = page, PageSize = pageSize, Items = orders.Select(MapOrderToResponse) };
                return Results.Ok(response);
            });

    private static IQueryable<MarineLaceSpace.Models.Database.Order.Order> ApplyFilters(
        IQueryable<MarineLaceSpace.Models.Database.Order.Order> query, OrderFilterRequest filter)
    {
        if (filter.StatusId.HasValue)
            query = query.Where(o => o.StatusId == filter.StatusId.Value);
        if (filter.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);
        return query;
    }

    private static IQueryable<MarineLaceSpace.Models.Database.Order.Order> ApplySorting(
        IQueryable<MarineLaceSpace.Models.Database.Order.Order> query, string? sortBy, bool sortDesc)
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "totalprice" => sortDesc ? query.OrderByDescending(o => o.TotalPrice) : query.OrderBy(o => o.TotalPrice),
            "status" => sortDesc ? query.OrderByDescending(o => o.StatusId) : query.OrderBy(o => o.StatusId),
            _ => sortDesc ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt),
        };
    }

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
