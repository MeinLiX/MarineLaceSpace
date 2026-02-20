using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Basket;

namespace Basket.WebHost.Routes;

public static class BasketRoutes
{
    public static void MapBasketRoutes(this IEndpointRouteBuilder app)
    {
        var basketGroup = app.MapGroup("/api/basket")
            .WithTags("Basket");

        basketGroup.MapGet("/", BasketHandlers.GetBasketHandler)
            .WithSummary("Get current basket");

        basketGroup.MapPost("/items", BasketHandlers.AddItemHandler)
            .WithSummary("Add item to basket");

        basketGroup.MapPut("/items/{itemId}", BasketHandlers.UpdateItemHandler)
            .WithSummary("Update basket item quantity or personalization");

        basketGroup.MapDelete("/items/{itemId}", BasketHandlers.RemoveItemHandler)
            .WithSummary("Remove item from basket");

        basketGroup.MapDelete("/", BasketHandlers.ClearBasketHandler)
            .WithSummary("Clear the entire basket");

        basketGroup.MapPost("/checkout", BasketHandlers.CheckoutHandler)
            .WithSummary("Checkout basket and create order")
            .RequireAuthorization()
            .Produces<IRESTResult>(StatusCodes.Status200OK)
            .Produces<IRESTResult>(StatusCodes.Status400BadRequest)
            .Produces<IRESTResult>(StatusCodes.Status401Unauthorized);
    }
}
