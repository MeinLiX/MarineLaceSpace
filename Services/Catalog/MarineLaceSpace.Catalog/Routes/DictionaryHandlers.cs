using BB.Common.Routes;
using MarineLaceSpace.DTO.Requests.Catalog;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;
using MarineLaceSpace.Enumerations;
using MarineLaceSpace.Models.Database.Catalog;
using MarineLaceSpace.Models.Routes;
using MarineLaceSpace.Catalog.Data.DBContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using System.Security.Claims;

namespace Catalog.WebHost.Routes;

internal class DictionaryHandlers
{
    private record DictionaryServices : BasicRouteServices
    {
        public required CatalogDbContext DbContext { get; init; }
        public required ILogger<DictionaryHandlers> Logger { get; init; }
        public required IMinioClient MinioClient { get; init; }
        public required IHttpContextAccessor HttpContextAccessor { get; init; }
    }

    /// <summary>Resolve the effective shopId for filtering.
    /// Admin without shopId → null (means "all"). Admin with shopId → that shop.
    /// Seller → auto-resolved from their first shop. Unauthenticated → "" (global only).</summary>
    private static async Task<(string? shopId, bool isAdmin, bool isSeller)> ResolveShopScopeAsync(
        DictionaryServices services, string? queryShopId)
    {
        var httpContext = services.HttpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = httpContext?.User.IsInRole("Admin") ?? false;
        var isSeller = httpContext?.User.IsInRole("Seller") ?? false;

        if (isAdmin)
        {
            // Admin can pass a specific shopId to filter, or get everything
            return (queryShopId, true, false);
        }

        if (isSeller && !string.IsNullOrEmpty(userId))
        {
            if (!string.IsNullOrEmpty(queryShopId))
                return (queryShopId, false, true);

            // Auto-resolve the seller's first shop
            var shop = await services.DbContext.Shops
                .AsNoTracking()
                .Where(s => s.OwnerId == userId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();
            return (shop, false, true);
        }

        // Unauthenticated or no relevant role → global only (empty string sentinel)
        return ("", false, false);
    }

    /// <summary>Resolve shop ID for write operations. Returns (shopId, error).
    /// Admin without shopId → null (global). Admin with shopId → that shop.
    /// Seller → must auto-resolve to their shop; if none → error.</summary>
    private static async Task<(string? shopId, IResult? error)> ResolveWriteShopAsync(
        DictionaryServices services, string? requestShopId)
    {
        var httpContext = services.HttpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = httpContext?.User.IsInRole("Admin") ?? false;

        if (isAdmin)
        {
            return (requestShopId, null); // null = global, or specific shop
        }

        // Seller: ignore requestShopId and always use their own shop
        if (!string.IsNullOrEmpty(userId))
        {
            var shop = await services.DbContext.Shops
                .AsNoTracking()
                .Where(s => s.OwnerId == userId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(shop))
                return (null, Results.Forbid());

            return (shop, null);
        }

        return (null, Results.Unauthorized());
    }

    /// <summary>Check if a user can modify (update/delete) a dictionary entry.
    /// Admin can modify anything. Seller can only modify entries belonging to their shop.</summary>
    private static async Task<(bool allowed, IResult? error)> CanModifyEntryAsync(
        DictionaryServices services, string? entryShopId)
    {
        var httpContext = services.HttpContextAccessor.HttpContext;
        var isAdmin = httpContext?.User.IsInRole("Admin") ?? false;

        if (isAdmin) return (true, null);

        var userId = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return (false, Results.Unauthorized());

        // Global entry — seller cannot modify
        if (string.IsNullOrEmpty(entryShopId))
            return (false, Results.Forbid());

        // Shop-scoped entry — verify this seller owns the shop
        var ownsShop = await services.DbContext.Shops
            .AsNoTracking()
            .AnyAsync(s => s.Id == entryShopId && s.OwnerId == userId);

        return ownsShop ? (true, null) : (false, Results.Forbid());
    }

    internal static Delegate GetSizesHandler =>
        async (string? shopId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var (resolvedShopId, isAdmin, _) = await ResolveShopScopeAsync(services, shopId);

                IQueryable<Size> query = services.DbContext.Sizes.AsNoTracking();

                if (resolvedShopId == "")
                {
                    // Unauthenticated: global only
                    query = query.Where(s => s.ShopId == null);
                }
                else if (resolvedShopId != null)
                {
                    // Specific shop: global + that shop
                    query = query.Where(s => s.ShopId == null || s.ShopId == resolvedShopId);
                }
                // else: admin without filter → all entries

                var sizes = await query.ToListAsync();
                return Results.Ok(sizes.Select(s => new SizeResponse
                {
                    Id = s.Id, Name = s.Name, Description = s.Description,
                    IsCustom = s.IsCustom, Gender = s.Gender.Name, ShopId = s.ShopId
                }));
            });

    internal static Delegate CreateSizeHandler =>
        async (CreateSizeRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateSizeRequest, DictionaryServices>(request, sp,
                async (services) =>
                {
                    var (writeShopId, error) = await ResolveWriteShopAsync(services, request.ShopId);
                    if (error != null) return error;

                    var size = new Size
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = request.Name,
                        Description = request.Description ?? string.Empty,
                        IsCustom = request.IsCustom,
                        Gender = ProductSizeGender.FromId<ProductSizeGender>(request.GenderId) ?? ProductSizeGender.Unisex,
                        ShopId = writeShopId
                    };
                    await services.DbContext.Sizes.AddAsync(size);
                    await services.DbContext.SaveChangesAsync();
                    return Results.Created($"/api/sizes/{size.Id}", new SizeResponse
                    {
                        Id = size.Id, Name = size.Name, Description = size.Description,
                        IsCustom = size.IsCustom, Gender = size.Gender.Name, ShopId = size.ShopId
                    });
                });

    internal static Delegate UpdateSizeHandler =>
        async (string id, CreateSizeRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateSizeRequest, DictionaryServices>(request, sp,
                async (services) =>
                {
                    var size = await services.DbContext.Sizes.FindAsync(id);
                    if (size == null) return Results.NotFound(RESTResult.Fail("Size not found."));

                    var (allowed, error) = await CanModifyEntryAsync(services, size.ShopId);
                    if (!allowed) return error!;

                    size.Name = request.Name;
                    size.Description = request.Description ?? string.Empty;
                    size.IsCustom = request.IsCustom;
                    size.Gender = ProductSizeGender.FromId<ProductSizeGender>(request.GenderId) ?? ProductSizeGender.Unisex;
                    await services.DbContext.SaveChangesAsync();
                    return Results.Ok(new SizeResponse { Id = size.Id, Name = size.Name, Description = size.Description, IsCustom = size.IsCustom, Gender = size.Gender.Name, ShopId = size.ShopId });
                });

    internal static Delegate DeleteSizeHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var size = await services.DbContext.Sizes.FindAsync(id);
                if (size == null) return Results.NotFound(RESTResult.Fail("Size not found."));

                var (allowed, error) = await CanModifyEntryAsync(services, size.ShopId);
                if (!allowed) return error!;

                services.DbContext.Sizes.Remove(size);
                await services.DbContext.SaveChangesAsync();
                return Results.NoContent();
            });

    internal static Delegate GetColorsHandler =>
        async (string? shopId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var (resolvedShopId, isAdmin, _) = await ResolveShopScopeAsync(services, shopId);

                IQueryable<Color> query = services.DbContext.Colors.AsNoTracking();

                if (resolvedShopId == "")
                {
                    query = query.Where(c => c.ShopId == null);
                }
                else if (resolvedShopId != null)
                {
                    query = query.Where(c => c.ShopId == null || c.ShopId == resolvedShopId);
                }

                var colors = await query.ToListAsync();
                return Results.Ok(colors.Select(c => new ColorResponse { Id = c.Id, Name = c.Name, HexCode = c.HexCode, ShopId = c.ShopId }));
            });

    internal static Delegate CreateColorHandler =>
        async (CreateColorRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateColorRequest, DictionaryServices>(request, sp,
                async (services) =>
                {
                    var (writeShopId, error) = await ResolveWriteShopAsync(services, request.ShopId);
                    if (error != null) return error;

                    var color = new Color { Id = Guid.NewGuid().ToString(), Name = request.Name, HexCode = request.HexCode, ShopId = writeShopId };
                    await services.DbContext.Colors.AddAsync(color);
                    await services.DbContext.SaveChangesAsync();
                    return Results.Created($"/api/colors/{color.Id}", new ColorResponse { Id = color.Id, Name = color.Name, HexCode = color.HexCode, ShopId = color.ShopId });
                });

    internal static Delegate UpdateColorHandler =>
        async (string id, CreateColorRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateColorRequest, DictionaryServices>(request, sp,
                async (services) =>
                {
                    var color = await services.DbContext.Colors.FindAsync(id);
                    if (color == null) return Results.NotFound(RESTResult.Fail("Color not found."));

                    var (allowed, error) = await CanModifyEntryAsync(services, color.ShopId);
                    if (!allowed) return error!;

                    color.Name = request.Name;
                    color.HexCode = request.HexCode;
                    await services.DbContext.SaveChangesAsync();
                    return Results.Ok(new ColorResponse { Id = color.Id, Name = color.Name, HexCode = color.HexCode, ShopId = color.ShopId });
                });

    internal static Delegate DeleteColorHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var color = await services.DbContext.Colors.FindAsync(id);
                if (color == null) return Results.NotFound(RESTResult.Fail("Color not found."));

                var (allowed, error) = await CanModifyEntryAsync(services, color.ShopId);
                if (!allowed) return error!;

                services.DbContext.Colors.Remove(color);
                await services.DbContext.SaveChangesAsync();
                return Results.NoContent();
            });

    internal static Delegate GetMaterialsHandler =>
        async (string? shopId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var (resolvedShopId, isAdmin, _) = await ResolveShopScopeAsync(services, shopId);

                IQueryable<Material> query = services.DbContext.Materials.AsNoTracking();

                if (resolvedShopId == "")
                {
                    query = query.Where(m => m.ShopId == null);
                }
                else if (resolvedShopId != null)
                {
                    query = query.Where(m => m.ShopId == null || m.ShopId == resolvedShopId);
                }

                var materials = await query.ToListAsync();
                return Results.Ok(materials.Select(m => new MaterialResponse { Id = m.Id, Name = m.Name, Description = m.Description, ImageUrl = m.ImageUrl, ShopId = m.ShopId }));
            });

    private static async Task<string?> UploadMaterialImageAsync(IMinioClient? minioClient, IFormFile file)
    {
        if (minioClient == null)
            throw new InvalidOperationException("MinIO client is not configured. Cannot upload material image.");

        var bucketName = "materials";
        var beArgs = new BucketExistsArgs().WithBucket(bucketName);
        bool found = await minioClient.BucketExistsAsync(beArgs);
        if (!found)
        {
            var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
            await minioClient.MakeBucketAsync(mbArgs);

            var policy = $@"{{""Version"":""2012-10-17"",""Statement"":[{{""Action"":[""s3:GetObject""],""Effect"":""Allow"",""Principal"":{{""AWS"":[""*""]}},""Resource"":[""arn:aws:s3:::{bucketName}/*""]}}]}}";
            await minioClient.SetPolicyAsync(new SetPolicyArgs().WithBucket(bucketName).WithPolicy(policy));
        }

        var objectName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        using var stream = file.OpenReadStream();
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(file.ContentType);
        await minioClient.PutObjectAsync(putObjectArgs);

        return $"/minio/{bucketName}/{objectName}";
    }

    internal static Delegate CreateMaterialHandler =>
        async ([FromForm] string name, [FromForm] string? description, [FromForm] string? shopId, IFormFile? imageFile, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var (writeShopId, error) = await ResolveWriteShopAsync(services, shopId);
                if (error != null) return error;

                string? imageUrl = null;
                if (imageFile is { Length: > 0 })
                {
                    imageUrl = await UploadMaterialImageAsync(services.MinioClient, imageFile);
                }

                var material = new Material
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Description = description ?? string.Empty,
                    ImageUrl = imageUrl,
                    ShopId = writeShopId
                };
                await services.DbContext.Materials.AddAsync(material);
                await services.DbContext.SaveChangesAsync();
                return Results.Created($"/api/materials/{material.Id}",
                    new MaterialResponse { Id = material.Id, Name = material.Name, Description = material.Description, ImageUrl = material.ImageUrl, ShopId = material.ShopId });
            });

    internal static Delegate UpdateMaterialHandler =>
        async (string id, [FromForm] string name, [FromForm] string? description, IFormFile? imageFile, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var material = await services.DbContext.Materials.FindAsync(id);
                if (material == null) return Results.NotFound(RESTResult.Fail("Material not found."));

                var (allowed, error) = await CanModifyEntryAsync(services, material.ShopId);
                if (!allowed) return error!;

                material.Name = name;
                material.Description = description ?? string.Empty;

                if (imageFile is { Length: > 0 })
                {
                    material.ImageUrl = await UploadMaterialImageAsync(services.MinioClient, imageFile);
                }

                await services.DbContext.SaveChangesAsync();
                return Results.Ok(new MaterialResponse { Id = material.Id, Name = material.Name, Description = material.Description, ImageUrl = material.ImageUrl, ShopId = material.ShopId });
            });

    internal static Delegate DeleteMaterialHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var material = await services.DbContext.Materials.FindAsync(id);
                if (material == null) return Results.NotFound(RESTResult.Fail("Material not found."));

                var (allowed, error) = await CanModifyEntryAsync(services, material.ShopId);
                if (!allowed) return error!;

                services.DbContext.Materials.Remove(material);
                await services.DbContext.SaveChangesAsync();
                return Results.NoContent();
            });
}
