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
                        if (shop.OwnerId != currentUserId) return Results.Forbid();
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
                        if (productToUpdate.Shop.OwnerId != currentUserId) return Results.Forbid();

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

                        if (productToDelete.Shop.OwnerId != currentUserId)
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
                        if (product.Shop.OwnerId != currentUserId)
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
                        if (product.Shop.OwnerId != currentUserId)
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