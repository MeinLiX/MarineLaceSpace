using MarineLaceSpace.DTO.Responses.Catalog;

namespace Catalog.WebHost.Routes;

public static class PhotoRoutes
{
    public static void MapPhotoRoutes(this IEndpointRouteBuilder app)
    {
        var photosGroup = app.MapGroup("/api").WithTags("Product Photos");

        photosGroup.MapGet("/products/{productId}/images", PhotoHandlers.GetProductPhotosHandler)
            .WithSummary("Get all photos for a product");

        photosGroup.MapGet("/products/{productId}/images/{imageId}", PhotoHandlers.GetPhotoByIdHandler)
            .WithSummary("Get a photo by ID")
            .Produces<ProductPhotoResponse>();

        photosGroup.MapPost("/shops/{shopId}/products/{productId}/images", PhotoHandlers.UploadPhotoHandler)
            .WithSummary("Upload a photo for a product")
            .RequireAuthorization("SellersOrAdmin");

        photosGroup.MapDelete("/shops/{shopId}/products/{productId}/images/{imageId}", PhotoHandlers.DeletePhotoHandler)
            .WithSummary("Delete a product photo")
            .RequireAuthorization("SellersOrAdmin");

        photosGroup.MapPut("/images/{imageId}", PhotoHandlers.UpdatePhotoHandler)
            .WithSummary("Update photo metadata")
            .RequireAuthorization("SellersOrAdmin");
    }
}
