using BB.Common.Extensions;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;

namespace Catalog.WebHost.Routes;

public static class ProductRoutes
{
    public static void MapProductRoutes(this IEndpointRouteBuilder app)
    {
        var productsGroup = app.MapGroup("/api/v1")
            .WithTags("Products");

        productsGroup.MapPost("/shops/{shopId}/products", ProductHandlers.CreateProductHandler)
            .WithSummary("Create a new product in a shop")
            .Produces< IRESTResult<ProductDetailResponse>>()
            .RequireAuthorization("SellersOrAdmin");

        productsGroup.MapGet("/shops/{shopId}/products", ProductHandlers.GetProductsByShopHandler)
            .WithSummary("Get all products in a shop")
            .Produces<IEnumerable<ProductSummaryResponse>>();

        productsGroup.MapGet("/products/{productId}", ProductHandlers.GetProductByIdHandler)
            .WithName("GetProductById")
            .WithSummary("Get a single product by its ID")
            .Produces< IRESTResult<ProductDetailResponse>>()
            .Produces<IRESTResult>(StatusCodes.Status404NotFound);

        productsGroup.MapPut("/products/{productId}", ProductHandlers.UpdateProductHandler)
            .WithSummary("Update a product")
            .Produces< IRESTResult<ProductDetailResponse>>()
            .AddValidationResponseType()
            .RequireAuthorization("SellersOrAdmin");

        productsGroup.MapDelete("/products/{productId}", ProductHandlers.DeleteProductHandler)
            .WithSummary("Delete a product")
            .Produces<IRESTResult>(StatusCodes.Status204NoContent)
            .RequireAuthorization("SellersOrAdmin");

        productsGroup.MapPut("/products/{productId}/inventory", ProductHandlers.UpdateInventoryHandler)
            .WithSummary("Update product inventory and prices for all variations")
            .Produces<IRESTResult>(StatusCodes.Status204NoContent)
            .Produces<IRESTResult>(StatusCodes.Status403Forbidden)
            .Produces<IRESTResult>(StatusCodes.Status404NotFound)
            .AddValidationResponseType()
            .RequireAuthorization("SellersOrAdmin");

        productsGroup.MapPut("/products/{productId}/variation-images", ProductHandlers.AssociateVariationImagesHandler)
            .WithSummary("Update product variation image associations")
            .Produces<IRESTResult>(StatusCodes.Status204NoContent)
            .Produces<IRESTResult>(StatusCodes.Status403Forbidden)
            .Produces<IRESTResult>(StatusCodes.Status404NotFound)
            .RequireAuthorization("SellersOrAdmin");

        productsGroup.MapGet("/products/admin", ProductHandlers.GetAllProductsAdminHandler)
            .WithSummary("Get all products for admin panel with filters and pagination")
            .Produces<IRESTResult>(StatusCodes.Status200OK)
            .RequireAuthorization("AdminOnly");

        productsGroup.MapGet("/products/active", ProductHandlers.GetActiveProductsHandler)
            .WithSummary("Get active products with optional filters and pagination")
            .Produces<IRESTResult>(StatusCodes.Status200OK);

        productsGroup.MapGet("/products/search", ProductHandlers.SearchProductsHandler)
            .WithSummary("Search products by name, description, or tags");

        productsGroup.MapPost("/products/batch", ProductHandlers.GetProductsBatchHandler)
            .WithSummary("Get multiple products by IDs")
            .Produces<IRESTResult>(StatusCodes.Status200OK);

        productsGroup.MapGet("/products/{productId}/inventory", ProductHandlers.GetProductInventoryHandler)
            .WithSummary("Get inventory details for a specific product")
            .Produces<IRESTResult>(StatusCodes.Status200OK)
            .Produces<IRESTResult>(StatusCodes.Status404NotFound);
    }
}