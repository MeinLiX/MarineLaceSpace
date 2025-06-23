using BB.Common.Extensions;
using MarineLaceSpace.DTO.Responses.Catalog;
using MarineLaceSpace.Models.Database.Catalog;

namespace MarineLaceSpace.Catalog.Routes;

public static class ProductRoutes
{
    public static void MapProductRoutes(this IEndpointRouteBuilder app)
    {
        var productsGroup = app.MapGroup("/api/v1")
            .WithTags("Products");

        productsGroup.MapPost("/shops/{shopId}/products", ProductHandlers.CreateProductHandler)
            .WithSummary("Create a new product in a shop")
            .Produces<Product>()
            .RequireAuthorization("SellersOnly");

        productsGroup.MapGet("/shops/{shopId}/products", ProductHandlers.GetProductsByShopHandler)
            .WithSummary("Get all products in a shop")
            .Produces<IEnumerable<ProductSummaryResponse>>();

        productsGroup.MapGet("/products/{productId}", ProductHandlers.GetProductByIdHandler)
            .WithName("GetProductById")
            .WithSummary("Get a single product by its ID")
            .Produces<ProductDetailResponse>()
            .Produces(StatusCodes.Status404NotFound);

        productsGroup.MapPut("/products/{productId}", ProductHandlers.UpdateProductHandler)
            .WithSummary("Update a product")
            .Produces<Product>()
            .AddValidationResponseType()
            .RequireAuthorization("SellersOnly");

        productsGroup.MapDelete("/products/{productId}", ProductHandlers.DeleteProductHandler)
            .WithSummary("Delete a product")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization("SellersOnly");

        productsGroup.MapPut("/products/{productId}/inventory", ProductHandlers.UpdateInventoryHandler)
            .WithSummary("Update product inventory and prices for all variations")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .AddValidationResponseType()
            .RequireAuthorization("SellersOnly");

        productsGroup.MapPut("/products/{productId}/variation-images", ProductHandlers.AssociateVariationImagesHandler)
            .WithSummary("Update product variation image associations")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("SellersOnly");
    }
}