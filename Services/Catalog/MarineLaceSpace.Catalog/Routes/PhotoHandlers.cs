using BB.Common.Routes;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using MarineLaceSpace.Models.Database.Catalog;
using MarineLaceSpace.Models.Routes;
using System.Security.Claims;

namespace Catalog.WebHost.Routes;

internal class PhotoHandlers
{
    private record PhotoServices : BasicRouteServices
    {
        public required IProductPhotoRepository PhotoRepository { get; init; }
        public required IProductRepository ProductRepository { get; init; }
        public required IHttpContextAccessor HttpContextAccessor { get; init; }
        public required ILogger<PhotoHandlers> Logger { get; init; }
    }

    internal static Delegate GetProductPhotosHandler =>
        async (string productId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<PhotoServices>(sp, async (services) =>
            {
                var photos = await services.PhotoRepository.GetByProductIdAsync(productId);
                return Results.Ok(photos.Select(MapPhotoToResponse));
            });

    internal static Delegate GetPhotoByIdHandler =>
        async (string productId, string imageId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<PhotoServices>(sp, async (services) =>
            {
                try
                {
                    var photo = await services.PhotoRepository.GetByIdAsync(imageId);
                    return Results.Ok(MapPhotoToResponse(photo));
                }
                catch (NotFoundEntityException ex)
                {
                    return Results.NotFound(RESTResult.Fail(ex.Message));
                }
            });

    internal static Delegate UploadPhotoHandler =>
        async (string shopId, string productId, string url, string? title, bool isMain, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<PhotoServices>(sp, async (services) =>
            {
                var httpContext = services.HttpContextAccessor.HttpContext!;
                var currentUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                try
                {
                    var product = await services.ProductRepository.GetByIdAsync(productId);
                    if (product.Shop.OwnerId != currentUserId) return Results.Forbid();

                    var photo = new ProductPhoto
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = productId,
                        Url = url,
                        Title = title,
                        IsMain = isMain,
                        SortOrder = (await services.PhotoRepository.GetByProductIdAsync(productId)).Count()
                    };

                    var created = await services.PhotoRepository.AddAsync(photo);
                    return Results.Created($"/api/products/{productId}/images/{created.Id}", MapPhotoToResponse(created));
                }
                catch (NotFoundEntityException ex)
                {
                    return Results.NotFound(RESTResult.Fail(ex.Message));
                }
            });

    internal static Delegate DeletePhotoHandler =>
        async (string shopId, string productId, string imageId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<PhotoServices>(sp, async (services) =>
            {
                var httpContext = services.HttpContextAccessor.HttpContext!;
                var currentUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                try
                {
                    var product = await services.ProductRepository.GetByIdAsync(productId);
                    if (product.Shop.OwnerId != currentUserId) return Results.Forbid();

                    await services.PhotoRepository.DeleteAsync(imageId);
                    return Results.NoContent();
                }
                catch (NotFoundEntityException ex)
                {
                    return Results.NotFound(RESTResult.Fail(ex.Message));
                }
            });

    private static ProductPhotoResponse MapPhotoToResponse(ProductPhoto p) => new()
    {
        Id = p.Id, Url = p.Url, IsMain = p.IsMain,
        SizeId = p.ProductSizeId, ColorId = p.ProductColorId, MaterialId = p.ProductMaterialId
    };
}
