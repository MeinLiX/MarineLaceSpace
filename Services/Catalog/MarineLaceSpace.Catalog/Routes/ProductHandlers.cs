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

internal class ProductHandlers
{
    private record ProductServices : BasicRouteServices
    {
        public required IShopRepository ShopRepository { get; init; }
        public required IProductRepository ProductRepository { get; init; }
        public required IHttpContextAccessor HttpContextAccessor { get; init; }
        public required ILogger<ProductHandlers> Logger { get; init; }
    }

    internal static Delegate CreateProductHandler =>
        async (string shopId, CreateProductRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateProductRequest, ProductServices>(request, sp,
                async (services) =>
                {
                    var httpContext = services.HttpContextAccessor.HttpContext!;
                    var currentUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(currentUserId)) return Results.Unauthorized();
                    try
                    {
                        var shop = await services.ShopRepository.GetByIdAsync(shopId);
                        var isAdmin = httpContext.User.IsInRole("Admin");
                        if (!isAdmin && shop.OwnerId != currentUserId) return Results.Forbid();
                    }
                    catch (NotFoundEntityException)
                    {
                        return Results.NotFound($"Shop with ID '{shopId}' not found.");
                    }

                    var newProduct = new Product
                    {
                        Id = Guid.NewGuid().ToString(),
                        ShopId = shopId,
                        Name = request.Name,
                        Description = request.Description,
                        CategoryId = request.CategoryId,
                        AllowPersonalization = request.AllowPersonalization
                    };

                    var initialInventory = new ProductPrice
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = newProduct.Id,
                        BasePrice = request.Price,
                        Quantity = request.Quantity
                    };
                    newProduct.ProductPrices.Add(initialInventory);

                    var createdProduct = await services.ProductRepository.AddAsync(newProduct);

                    var responseDto = MapProductToDetailResponse(createdProduct);
                    return Results.CreatedAtRoute("GetProductById", new { productId = responseDto.Id }, responseDto);
                });

    internal static Delegate GetProductByIdHandler =>
        async (string productId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ProductServices>(sp, async (services) =>
            {
                try
                {
                    var product = await services.ProductRepository.GetByIdAsync(productId);
                    var responseDto = MapProductToDetailResponse(product);
                    return Results.Ok(responseDto);
                }
                catch (NotFoundEntityException ex)
                {
                    return Results.NotFound(RESTResult.Fail(ex.Message));
                }
            });

    internal static Delegate GetProductsByShopHandler =>
        async (string shopId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ProductServices>(sp, async (services) =>
            {
                var products = await services.ProductRepository.GetByShopIdAsync(shopId);
                var response = products.Select(product => new ProductSummaryResponse
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.ProductPrices.Any() ? product.ProductPrices.Min(p => p.BasePrice) : 0,
                    MainImageUrl = product.Photos?.FirstOrDefault(p => p.IsMain)?.Url ?? product.Photos?.FirstOrDefault()?.Url
                });
                return Results.Ok(response);
            });

    internal static Delegate UpdateProductHandler =>
        async (string productId, UpdateProductRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<UpdateProductRequest, ProductServices>(request, sp,
                async (services) =>
                {
                    var httpContext = services.HttpContextAccessor.HttpContext!;
                    var currentUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    try
                    {
                        var productToUpdate = await services.ProductRepository.GetByIdAsync(productId);
                        var isAdmin = httpContext.User.IsInRole("Admin");
                        if (!isAdmin && productToUpdate.Shop.OwnerId != currentUserId) return Results.Forbid();

                        productToUpdate.Name = request.Name;
                        productToUpdate.Description = request.Description;
                        productToUpdate.CategoryId = request.CategoryId;
                        productToUpdate.AllowPersonalization = request.AllowPersonalization;
                        productToUpdate.UpdatedAt = DateTime.UtcNow;

                        var baseInventory = productToUpdate.ProductPrices.FirstOrDefault(p => p.ProductSizeId == null && p.ProductColorId == null && p.ProductMaterialId == null);
                        if (baseInventory != null)
                        {
                            baseInventory.BasePrice = request.Price;
                            baseInventory.Quantity = request.Quantity;
                        }

                        await services.ProductRepository.UpdateAsync(productToUpdate);

                        var responseDto = MapProductToDetailResponse(productToUpdate);
                        return Results.Ok(responseDto);
                    }
                    catch (NotFoundEntityException ex)
                    {
                        return Results.NotFound(RESTResult.Fail(ex.Message));
                    }
                });

    internal static Delegate DeleteProductHandler =>
        async (string productId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ProductServices>(sp,
                async (services) =>
                {
                    var httpContext = services.HttpContextAccessor.HttpContext!;
                    var currentUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    try
                    {
                        var productToDelete = await services.ProductRepository.GetByIdAsync(productId);
                        var isAdmin = httpContext.User.IsInRole("Admin");

                        if (!isAdmin && productToDelete.Shop.OwnerId != currentUserId)
                        {
                            return Results.Forbid();
                        }

                        await services.ProductRepository.DeleteAsync(productId);
                        return Results.NoContent();
                    }
                    catch (NotFoundEntityException ex)
                    {
                        return Results.NotFound(RESTResult.Fail(ex.Message));
                    }
                });

    internal static Delegate UpdateInventoryHandler =>
        async (string productId, UpdateProductInventoryRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<UpdateProductInventoryRequest, ProductServices>(request, sp,
                async (services) =>
                {
                    var httpContext = services.HttpContextAccessor.HttpContext!;
                    var currentUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    try
                    {
                        var product = await services.ProductRepository.GetByIdAsync(productId);
                        var isAdmin = httpContext.User.IsInRole("Admin");
                        if (!isAdmin && product.Shop.OwnerId != currentUserId)
                        {
                            return Results.Forbid();
                        }

                        var newInventory = request.InventoryItems.Select(item => new ProductPrice
                        {
                            Id = Guid.NewGuid().ToString(),
                            ProductId = productId,
                            ProductSizeId = item.SizeId,
                            ProductColorId = item.ColorId,
                            ProductMaterialId = item.MaterialId,
                            BasePrice = item.Price,
                            Quantity = item.Quantity
                        }).ToList();

                        await services.ProductRepository.UpdateInventoryAsync(productId, newInventory);

                        return Results.NoContent();
                    }
                    catch (NotFoundEntityException ex)
                    {
                        return Results.NotFound(RESTResult.Fail(ex.Message));
                    }
                });
    internal static Delegate AssociateVariationImagesHandler =>
        async (string productId, UpdateVariationImagesRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<UpdateVariationImagesRequest, ProductServices>(request, sp,
                async (services) =>
                {
                    var httpContext = services.HttpContextAccessor.HttpContext!;
                    var currentUserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    try
                    {
                        var product = await services.ProductRepository.GetByIdAsync(productId);
                        var isAdmin = httpContext.User.IsInRole("Admin");
                        if (!isAdmin && product.Shop.OwnerId != currentUserId)
                        {
                            return Results.Forbid();
                        }

                        await services.ProductRepository.AssociateImagesWithVariationsAsync(productId, request.Associations);

                        return Results.NoContent();
                    }
                    catch (NotFoundEntityException ex)
                    {
                        return Results.NotFound(RESTResult.Fail(ex.Message));
                    }
                });


    internal static Delegate GetAllProductsAdminHandler =>
        async (string? search, string? categoryId, string? shopId, int? page, int? pageSize, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ProductServices>(sp, async services =>
            {
                try
                {
                    var dbContext = sp.GetRequiredService<CatalogDbContext>();
                    var query = dbContext.Products
                        .Include(p => p.Photos)
                        .Include(p => p.ProductPrices)
                        .Include(p => p.Shop)
                        .Include(p => p.Category)
                        .AsNoTracking();

                    if (!string.IsNullOrEmpty(search))
                        query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));

                    if (!string.IsNullOrEmpty(categoryId))
                        query = query.Where(p => p.CategoryId == categoryId);

                    if (!string.IsNullOrEmpty(shopId))
                        query = query.Where(p => p.ShopId == shopId);

                    var totalCount = await query.CountAsync();
                    var pg = Math.Max(1, page ?? 1);
                    var ps = Math.Clamp(pageSize ?? 20, 1, 100);

                    var products = await query
                        .OrderByDescending(p => p.CreatedAt)
                        .Skip((pg - 1) * ps)
                        .Take(ps)
                        .ToListAsync();

                    var response = new
                    {
                        Items = products.Select(p => new
                        {
                            p.Id,
                            p.Name,
                            p.Description,
                            p.CategoryId,
                            CategoryName = p.Category?.Name,
                            p.ShopId,
                            ShopName = p.Shop?.Name,
                            p.IsActive,
                            p.AllowPersonalization,
                            Price = p.ProductPrices.Any() ? p.ProductPrices.Min(pp => pp.BasePrice) : 0,
                            MainImageUrl = p.Photos.FirstOrDefault(ph => ph.IsMain)?.Url ?? p.Photos.FirstOrDefault()?.Url,
                            p.CreatedAt,
                            p.UpdatedAt
                        }),
                        TotalCount = totalCount,
                        Page = pg,
                        PageSize = ps,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)ps)
                    };

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(RESTResult.Fail(ex.Message));
                }
            });


    internal static Delegate GetActiveProductsHandler =>
        async (string? categoryId, string? search, decimal? minPrice, decimal? maxPrice,
            int page, int pageSize, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ProductServices>(sp, async services =>
            {
                try
                {
                    var dbContext = sp.GetRequiredService<CatalogDbContext>();
                    var query = dbContext.Products
                        .Where(p => p.IsActive)
                        .Include(p => p.Photos)
                        .Include(p => p.ProductPrices)
                        .AsNoTracking();

                    if (!string.IsNullOrEmpty(categoryId))
                        query = query.Where(p => p.CategoryId == categoryId);

                    if (!string.IsNullOrEmpty(search))
                        query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));

                    if (minPrice.HasValue)
                        query = query.Where(p => p.ProductPrices.Any(pp => pp.BasePrice >= minPrice.Value));

                    if (maxPrice.HasValue)
                        query = query.Where(p => p.ProductPrices.Any(pp => pp.BasePrice <= maxPrice.Value));

                    var totalCount = await query.CountAsync();

                    var clampedPage = Math.Max(1, page);
                    var clampedSize = Math.Clamp(pageSize, 1, 50);

                    var products = await query
                        .OrderByDescending(p => p.CreatedAt)
                        .Skip((clampedPage - 1) * clampedSize)
                        .Take(clampedSize)
                        .ToListAsync();

                    var response = new
                    {
                        Items = products.Select(p => new ProductSummaryResponse
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Price = p.ProductPrices.FirstOrDefault()?.BasePrice ?? 0,
                            MainImageUrl = p.Photos.FirstOrDefault(ph => ph.IsMain)?.Url ?? p.Photos.FirstOrDefault()?.Url
                        }),
                        TotalCount = totalCount,
                        Page = clampedPage,
                        PageSize = clampedSize,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)clampedSize)
                    };

                    return Results.Ok(RESTResult<object>.Success(response));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(RESTResult.Fail(ex.Message));
                }
            });

    internal static Delegate GetProductsBatchHandler =>
        async (List<string> ids, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ProductServices>(sp, async services =>
            {
                try
                {
                    var dbContext = sp.GetRequiredService<CatalogDbContext>();
                    var products = await dbContext.Products
                        .Where(p => ids.Contains(p.Id))
                        .Include(p => p.Photos)
                        .Include(p => p.ProductPrices)
                        .AsNoTracking()
                        .ToListAsync();

                    var response = products.Select(p => new ProductSummaryResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.ProductPrices.FirstOrDefault()?.BasePrice ?? 0,
                        MainImageUrl = p.Photos.FirstOrDefault(ph => ph.IsMain)?.Url ?? p.Photos.FirstOrDefault()?.Url
                    });

                    return Results.Ok(RESTResult<IEnumerable<ProductSummaryResponse>>.Success(response));
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(RESTResult.Fail(ex.Message));
                }
            });

    internal static Delegate GetProductInventoryHandler =>
        async (string productId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ProductServices>(sp, async services =>
            {
                try
                {
                    var dbContext = sp.GetRequiredService<CatalogDbContext>();
                    var inventory = await dbContext.ProductPrices
                        .Where(pp => pp.ProductId == productId)
                        .AsNoTracking()
                        .Select(pp => new ProductInventoryItemResponse
                        {
                            SizeId = pp.ProductSizeId,
                            ColorId = pp.ProductColorId,
                            MaterialId = pp.ProductMaterialId,
                            Price = pp.BasePrice,
                            Quantity = pp.Quantity
                        })
                        .ToListAsync();

                    return Results.Ok(RESTResult<List<ProductInventoryItemResponse>>.Success(inventory));
                }
                catch (Exception ex)
                {
                    return Results.NotFound(RESTResult.Fail(ex.Message));
                }
            });

    internal static Delegate SearchProductsHandler =>
        async (string q, int? page, int? pageSize, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<ProductServices>(sp, async (services) =>
            {
                if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                    return Results.BadRequest(RESTResult.Fail("Search query must be at least 2 characters."));

                var dbContext = sp.GetRequiredService<CatalogDbContext>();
                var searchTerm = q.Trim().ToLower();
                var query = dbContext.Products
                    .Where(p => p.IsActive && (
                        EF.Functions.ILike(p.Name, $"%{searchTerm}%") ||
                        EF.Functions.ILike(p.Description ?? "", $"%{searchTerm}%")
                    ));

                var totalCount = await query.CountAsync();
                var pg = page ?? 1;
                var ps = Math.Clamp(pageSize ?? 20, 1, 100);

                var products = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((pg - 1) * ps)
                    .Take(ps)
                    .Include(p => p.Photos)
                    .Include(p => p.ProductPrices)
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(new
                {
                    TotalCount = totalCount,
                    Page = pg,
                    PageSize = ps,
                    Query = q,
                    Items = products.Select(p => new ProductSummaryResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.ProductPrices.FirstOrDefault()?.BasePrice ?? 0,
                        MainImageUrl = p.Photos.FirstOrDefault(ph => ph.IsMain)?.Url ?? p.Photos.FirstOrDefault()?.Url
                    })
                });
            });

    #region 
    private static ProductDetailResponse MapProductToDetailResponse(Product product)
    {
        var inventoryResponse = product.ProductPrices?.Select(price => new ProductInventoryItemResponse
        {
            SizeId = price.ProductSizeId,
            ColorId = price.ProductColorId,
            MaterialId = price.ProductMaterialId,
            Price = price.BasePrice,
            Quantity = price.Quantity
        }).ToList() ?? [];

        return new ProductDetailResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            TotalQuantity = inventoryResponse.Sum(i => i.Quantity),
            AllowPersonalization = product.AllowPersonalization,
            ShopId = product.ShopId,
            MainImageUrl = product.Photos?.FirstOrDefault(p => p.IsMain)?.Url ?? product.Photos?.FirstOrDefault()?.Url,
            Photos = product.Photos?.Select(p => new ProductPhotoResponse
            {
                Id = p.Id,
                Url = p.Url,
                IsMain = p.IsMain,
                SizeId = p.ProductSizeId,
                ColorId = p.ProductColorId,
                MaterialId = p.ProductMaterialId
            }).ToList() ?? [],
            Price = inventoryResponse.Count != 0 ? inventoryResponse.Min(i => i.Price) : 0,
            Inventory = inventoryResponse
        };
    }
    #endregion
}