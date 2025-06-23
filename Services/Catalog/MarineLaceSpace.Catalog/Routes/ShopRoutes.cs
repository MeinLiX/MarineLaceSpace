using BB.Common.Extensions;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;
using MarineLaceSpace.Models.Database.Catalog;

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
            .Produces<Shop>(StatusCodes.Status201Created)
            .Produces<RESTErrorResult<object>>(StatusCodes.Status401Unauthorized)
            .Produces<RESTErrorResult<object>>(StatusCodes.Status403Forbidden)
            .Produces<RESTErrorResult<object>>(StatusCodes.Status409Conflict)
            .AddValidationResponseType()
            .RequireAuthorization("SellersOnly");

        shopsGroup.MapGet("/{id}", ShopHandlers.GetShopByIdHandler)
            .WithName("GetShopById")
            .WithSummary("Get a shop by ID")
            .Produces<ShopResponse>(StatusCodes.Status200OK)
            .Produces<RESTErrorResult<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization("SellersOnly");

        shopsGroup.MapGet("/slug/{urlSlug}", ShopHandlers.GetShopBySlugHandler)
            .WithName("GetShopBySlug")
            .WithSummary("Get a shop by its URL slug")
            .Produces<ShopResponse>(StatusCodes.Status200OK)
            .Produces<RESTErrorResult<object>>(StatusCodes.Status404NotFound);

        shopsGroup.MapPut("/{id}", ShopHandlers.UpdateShopHandler)
            .WithSummary("Update a shop")
            .WithDescription("Updates a shop's details. The user must be the owner of the shop.")
            .Produces<Shop>(StatusCodes.Status200OK)
            .Produces<RESTErrorResult<object>>(StatusCodes.Status401Unauthorized)
            .Produces<RESTErrorResult<object>>(StatusCodes.Status403Forbidden)
            .Produces<RESTErrorResult<object>>(StatusCodes.Status404NotFound)
            .AddValidationResponseType()
            .RequireAuthorization("SellersOnly");

        shopsGroup.MapDelete("/{id}", ShopHandlers.DeleteShopHandler)
            .WithSummary("Delete a shop")
            .WithDescription("Deletes a shop. The user must be the owner of the shop.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<RESTErrorResult<object>>(StatusCodes.Status401Unauthorized)
            .Produces<RESTErrorResult<object>>(StatusCodes.Status403Forbidden)
            .Produces<RESTErrorResult<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization("SellersOnly");
    }
}