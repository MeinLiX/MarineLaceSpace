using BB.Common.Routes;
using MarineLaceSpace.DTO.Requests.Catalog;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using MarineLaceSpace.Models.Database.Catalog;
using MarineLaceSpace.Models.Routes;
using System.Security.Claims;

namespace Catalog.WebHost.Routes;

internal class ShopHandlers
{
    private record ShopServices : BasicRouteServices
    {
        public required IShopRepository ShopRepository { get; init; }
        public required IHttpContextAccessor HttpContextAccessor { get; init; }
        public required ILogger<ShopHandlers> Logger { get; init; }
    }

    internal static Delegate CreateShopHandler =>
        async (CreateShopRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateShopRequest, ShopServices>(request, sp,
                async (services) =>
                {
                    var httpContext = services.HttpContextAccessor.HttpContext;

                    if (httpContext is null)
                    {
                        services.Logger.LogError("HttpContext is not available in the current context.");
                        return Results.Problem("Internal Server Error: HttpContext is not available.");
                    }

                    var ownerId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (string.IsNullOrEmpty(ownerId))
                    {
                        return Results.Unauthorized();
                    }

                    services.Logger.LogInformation("Attempting to create shop '{ShopName}' for user {OwnerId}", request.Name, ownerId);

                    try
                    {
                        await services.ShopRepository.GetByOwnerIdAsync(ownerId);
                        return Results.Conflict(RESTResult<object>.Fail("This user already owns a shop."));
                    }
                    catch (NotFoundEntityException)
                    {
                    }

                    var newShop = new Shop
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = request.Name,
                        Description = request.Description,
                        UrlSlug = request.UrlSlug,
                        OwnerId = ownerId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var createdShop = await services.ShopRepository.AddAsync(newShop);

                    return Results.CreatedAtRoute("GetShopById", new { id = createdShop.Id }, createdShop);
                });

    internal static Delegate GetShopByIdHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ShopServices>(sp,
                async (services) =>
                {
                    try
                    {
                        services.Logger.LogInformation("Attempting to retrieve shop with ID {ShopId}", id);
                        var shop = await services.ShopRepository.GetByIdAsync(id);

                        var shopResponse = new ShopResponse
                        {
                            Id = shop.Id,
                            Name = shop.Name,
                            Description = shop.Description,
                            UrlSlug = shop.UrlSlug,
                            LogoUrl = shop.LogoUrl,
                            BannerUrl = shop.BannerUrl,
                            CreatedAt = shop.CreatedAt
                        };

                        return Results.Ok(shopResponse);
                    }
                    catch (NotFoundEntityException ex)
                    {
                        services.Logger.LogWarning(ex, "Shop with ID {ShopId} not found", id);
                        return Results.NotFound(RESTResult<object>.Fail(ex.Message));
                    }
                });

    internal static Delegate GetShopBySlugHandler =>
        async (string urlSlug, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ShopServices>(sp,
                async (services) =>
                {
                    try
                    {
                        services.Logger.LogInformation("Attempting to retrieve shop with slug '{UrlSlug}'", urlSlug);
                        var shop = await services.ShopRepository.GetBySlugAsync(urlSlug);

                        var shopResponse = new ShopResponse
                        {
                            Id = shop.Id,
                            Name = shop.Name,
                            Description = shop.Description,
                            UrlSlug = shop.UrlSlug,
                            LogoUrl = shop.LogoUrl,
                            BannerUrl = shop.BannerUrl,
                            CreatedAt = shop.CreatedAt
                        };

                        return Results.Ok(shopResponse);
                    }
                    catch (NotFoundEntityException ex)
                    {
                        services.Logger.LogWarning(ex, "Shop with slug '{UrlSlug}' not found", urlSlug);
                        return Results.NotFound(RESTResult<object>.Fail(ex.Message));
                    }
                });

    internal static Delegate UpdateShopHandler =>
        async (string id, UpdateShopRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<UpdateShopRequest, ShopServices>(request, sp,
                async (services) =>
                {
                    var httpContext = services.HttpContextAccessor.HttpContext;
                    if (httpContext is null)
                    {
                        services.Logger.LogError("HttpContext is not available in the current context.");
                        return Results.Problem("Internal Server Error: HttpContext is not available.");
                    }

                    var currentUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(currentUserId))
                    {
                        return Results.Unauthorized();
                    }

                    try
                    {
                        var shopToUpdate = await services.ShopRepository.GetByIdAsync(id);

                        if (shopToUpdate.OwnerId != currentUserId)
                        {
                            services.Logger.LogWarning("User {CurrentUserId} attempted to update shop {ShopId} owned by {OwnerId}.", currentUserId, id, shopToUpdate.OwnerId);
                            return Results.Forbid();
                        }

                        shopToUpdate.Name = request.Name;
                        shopToUpdate.Description = request.Description;
                        shopToUpdate.UpdatedAt = DateTime.UtcNow;

                        await services.ShopRepository.UpdateAsync(shopToUpdate);

                        services.Logger.LogInformation("Shop {ShopId} was successfully updated by user {CurrentUserId}", id, currentUserId);
                        return Results.Ok(shopToUpdate);
                    }
                    catch (NotFoundEntityException ex)
                    {
                        services.Logger.LogWarning(ex, "Attempted to update a non-existent shop with ID {ShopId}", id);
                        return Results.NotFound(RESTResult<object>.Fail(ex.Message));
                    }
                });

    internal static Delegate DeleteShopHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ShopServices>(sp,
                async (services) =>
                {
                    var httpContext = services.HttpContextAccessor.HttpContext;
                    if (httpContext is null)
                    {
                        services.Logger.LogError("HttpContext is not available in the current context.");
                        return Results.Problem("Internal Server Error: HttpContext is not available.");
                    }

                    var currentUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(currentUserId))
                    {
                        return Results.Unauthorized();
                    }

                    try
                    {
                        var shopToDelete = await services.ShopRepository.GetByIdAsync(id);

                        if (shopToDelete.OwnerId != currentUserId)
                        {
                            services.Logger.LogWarning("User {CurrentUserId} attempted to DELETE shop {ShopId} owned by {OwnerId}.", currentUserId, id, shopToDelete.OwnerId);
                            return Results.Forbid();
                        }

                        await services.ShopRepository.DeleteAsync(id);

                        services.Logger.LogInformation("Shop {ShopId} was successfully deleted by user {CurrentUserId}", id, currentUserId);

                        return Results.NoContent();
                    }
                    catch (NotFoundEntityException ex)
                    {
                        services.Logger.LogWarning(ex, "Attempted to delete a non-existent shop with ID {ShopId}", id);
                        return Results.NotFound(RESTResult<object>.Fail(ex.Message));
                    }
                });
}