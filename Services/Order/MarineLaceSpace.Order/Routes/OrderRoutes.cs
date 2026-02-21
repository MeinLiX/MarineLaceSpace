using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Order;

namespace Order.WebHost.Routes;

public static class OrderRoutes
{
    public static void MapOrderRoutes(this IEndpointRouteBuilder app)
    {
        var ordersGroup = app.MapGroup("/api/orders")
            .WithTags("Orders");

        ordersGroup.MapGet("/", OrderHandlers.GetMyOrdersHandler)
            .WithSummary("Get current user's orders")
            .RequireAuthorization();

        ordersGroup.MapGet("/admin", OrderHandlers.GetAllOrdersAdminHandler)
            .WithSummary("Get all orders for admin panel")
            .RequireAuthorization("AdminOnly");

        ordersGroup.MapGet("/{id}", OrderHandlers.GetOrderByIdHandler)
            .WithSummary("Get order by ID")
            .RequireAuthorization();

        ordersGroup.MapPut("/{id}/status", OrderHandlers.UpdateOrderStatusHandler)
            .WithSummary("Update order status")
            .RequireAuthorization("SellersOrAdmin");

        ordersGroup.MapPost("/{id}/tracking", OrderHandlers.AddTrackingHandler)
            .WithSummary("Add tracking number")
            .RequireAuthorization("SellersOrAdmin");

        ordersGroup.MapPost("/{id}/cancel", OrderHandlers.CancelOrderHandler)
            .WithSummary("Cancel an order")
            .RequireAuthorization();

        var shopOrdersGroup = app.MapGroup("/api/shops/{shopId}/orders")
            .WithTags("Orders");

        shopOrdersGroup.MapGet("/", OrderHandlers.GetShopOrdersHandler)
            .WithSummary("Get orders for a shop")
            .RequireAuthorization("SellersOrAdmin");

        shopOrdersGroup.MapGet("/statistics", OrderHandlers.GetShopOrderStatisticsHandler)
            .WithSummary("Get order statistics for a shop")
            .RequireAuthorization("SellersOrAdmin");
    }
}
