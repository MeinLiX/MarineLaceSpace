using BB.Common.Extensions;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;

namespace Catalog.WebHost.Routes;

public static class ShopRoutes
{
    public static void MapShopRoutes(this IEndpointRouteBuilder app)
    {
        var shopsGroup = app.MapGroup("shops")
            .WithTags("Shops");

        shopsGroup.MapPost("/", ShopHandlers.CreateShopHandler)
            .WithSummary("Create a new shop")
            .WithDescription("Registers a new shop for the authenticated user.")
            .Produces<IRESTResult<ShopResponse>>(StatusCodes.Status201Created)
            .Produces<IRESTResult>(StatusCodes.Status401Unauthorized)
            .Produces<IRESTResult>(StatusCodes.Status403Forbidden)
            .Produces<IRESTResult>(StatusCodes.Status409Conflict)
            .AddValidationResponseType()
            .RequireAuthorization("SellersOnly");

        shopsGroup.MapGet("/{id}", ShopHandlers.GetShopByIdHandler)
            .WithName("GetShopById")
            .WithSummary("Get a shop by ID")
            .Produces< IRESTResult<ShopResponse>>(StatusCodes.Status200OK)
            .Produces<IRESTResult>(StatusCodes.Status404NotFound)
            .RequireAuthorization("SellersOnly");

        shopsGroup.MapGet("/slug/{urlSlug}", ShopHandlers.GetShopBySlugHandler)
            .WithName("GetShopBySlug")
            .WithSummary("Get a shop by its URL slug")
            .Produces< IRESTResult<ShopResponse>>(StatusCodes.Status200OK)
            .Produces<IRESTResult>(StatusCodes.Status404NotFound);

        shopsGroup.MapPut("/{id}", ShopHandlers.UpdateShopHandler)
            .WithSummary("Update a shop")
            .WithDescription("Updates a shop's details. The user must be the owner of the shop.")
            .Produces< IRESTResult<ShopResponse>>(StatusCodes.Status200OK)
            .Produces<IRESTResult>(StatusCodes.Status401Unauthorized)
            .Produces<IRESTResult>(StatusCodes.Status403Forbidden)
            .Produces<IRESTResult>(StatusCodes.Status404NotFound)
            .AddValidationResponseType()
            .RequireAuthorization("SellersOnly");

        shopsGroup.MapDelete("/{id}", ShopHandlers.DeleteShopHandler)
            .WithSummary("Delete a shop")
            .WithDescription("Deletes a shop. The user must be the owner of the shop.")
            .Produces< IRESTResult>(StatusCodes.Status204NoContent)
            .Produces<IRESTResult>(StatusCodes.Status401Unauthorized)
            .Produces<IRESTResult>(StatusCodes.Status403Forbidden)
            .Produces<IRESTResult>(StatusCodes.Status404NotFound)
            .RequireAuthorization("SellersOnly");

        shopsGroup.MapGet("/by-owner/{userId}", ShopHandlers.GetShopsByOwnerHandler)
            .WithSummary("Get shops owned by a specific user")
            .Produces<IRESTResult>(StatusCodes.Status200OK);

        shopsGroup.MapGet("/{shopId}/reviews", ShopHandlers.GetShopReviewsHandler)
            .WithSummary("Get reviews for all products in a shop")
            .Produces<IRESTResult>(StatusCodes.Status200OK);
    }
}