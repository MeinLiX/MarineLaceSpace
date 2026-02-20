using BB.Common.Routes;
using MarineLaceSpace.Catalog.Data.DBContexts;
using MarineLaceSpace.DTO.Requests.Catalog;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using MarineLaceSpace.Models.Database.Catalog;
using MarineLaceSpace.Models.Routes;
using Microsoft.EntityFrameworkCore;
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
                        return Results.Conflict(RESTResult.Fail("This user already owns a shop."));
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
                    var responseDto = MapShopToResponse(createdShop);
                    return Results.CreatedAtRoute("GetShopById", new { id = responseDto.Id }, responseDto);
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

                        return Results.Ok(MapShopToResponse(shop));
                    }
                    catch (NotFoundEntityException ex)
                    {
                        services.Logger.LogWarning(ex, "Shop with ID {ShopId} not found", id);
                        return Results.NotFound(RESTResult.Fail(ex.Message));
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

                        return Results.Ok(MapShopToResponse(shop));
                    }
                    catch (NotFoundEntityException ex)
                    {
                        services.Logger.LogWarning(ex, "Shop with slug '{UrlSlug}' not found", urlSlug);
                        return Results.NotFound(RESTResult.Fail(ex.Message));
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
                        return Results.Ok(MapShopToResponse(shopToUpdate));
                    }
                    catch (NotFoundEntityException ex)
                    {
                        services.Logger.LogWarning(ex, "Attempted to update a non-existent shop with ID {ShopId}", id);
                        return Results.NotFound(RESTResult.Fail(ex.Message));
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
                        return Results.NotFound(RESTResult.Fail(ex.Message));
                    }
                });

    internal static Delegate GetShopsByOwnerHandler =>
        async (string userId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ShopServices>(sp, async services =>
            {
                try
                {
                    var dbContext = sp.GetRequiredService<CatalogDbContext>();
                    var shops = await dbContext.Shops
                        .Where(s => s.OwnerId == userId)
                        .AsNoTracking()
                        .Select(s => new ShopResponse
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Description = s.Description,
                            UrlSlug = s.UrlSlug,
                            LogoUrl = s.LogoUrl,
                            BannerUrl = s.BannerUrl,
                            CreatedAt = s.CreatedAt
                        })
                        .ToListAsync();

                    return Results.Ok(RESTResult<List<ShopResponse>>.Success(shops));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(RESTResult.Fail(ex.Message));
                }
            });

    internal static Delegate GetShopReviewsHandler =>
        async (string shopId, int page, int pageSize, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ShopServices>(sp, async services =>
            {
                try
                {
                    var dbContext = sp.GetRequiredService<CatalogDbContext>();

                    var clampedPage = Math.Max(1, page);
                    var clampedSize = Math.Clamp(pageSize, 1, 50);

                    var shopProductIds = await dbContext.Products
                        .Where(p => p.ShopId == shopId)
                        .Select(p => p.Id)
                        .ToListAsync();

                    var totalCount = await dbContext.ProductReviews
                        .Where(r => shopProductIds.Contains(r.ProductId))
                        .CountAsync();

                    var reviews = await dbContext.ProductReviews
                        .Where(r => shopProductIds.Contains(r.ProductId))
                        .OrderByDescending(r => r.CreatedAt)
                        .Skip((clampedPage - 1) * clampedSize)
                        .Take(clampedSize)
                        .AsNoTracking()
                        .Select(r => new ReviewResponse
                        {
                            Id = r.Id,
                            ProductId = r.ProductId,
                            Rating = r.Rating,
                            Comment = r.Comment,
                            UserName = r.UserName,
                            CreatedAt = r.CreatedAt,
                            IsVerified = r.IsVerified
                        })
                        .ToListAsync();

                    var response = new { Items = reviews, TotalCount = totalCount, Page = clampedPage, PageSize = clampedSize };
                    return Results.Ok(RESTResult<object>.Success(response));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(RESTResult.Fail(ex.Message));
                }
            });

    #region
    private static ShopResponse MapShopToResponse(Shop shop) => new()
    {
        Id = shop.Id,
        Name = shop.Name,
        Description = shop.Description,
        UrlSlug = shop.UrlSlug,
        LogoUrl = shop.LogoUrl,
        BannerUrl = shop.BannerUrl,
        CreatedAt = shop.CreatedAt
    };
    #endregion
}