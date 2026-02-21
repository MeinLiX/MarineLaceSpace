using BB.Common.Routes;
using MarineLaceSpace.DTO.Requests.Catalog;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;
using MarineLaceSpace.Enumerations;
using MarineLaceSpace.Models.Database.Catalog;
using MarineLaceSpace.Models.Routes;
using MarineLaceSpace.Catalog.Data.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Catalog.WebHost.Routes;

internal class DictionaryHandlers
{
    private record DictionaryServices : BasicRouteServices
    {
        public required CatalogDbContext DbContext { get; init; }
        public required ILogger<DictionaryHandlers> Logger { get; init; }
    }

    // === SIZES ===
    internal static Delegate GetSizesHandler =>
        async (IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var sizes = await services.DbContext.Sizes.AsNoTracking().ToListAsync();
                return Results.Ok(sizes.Select(s => new SizeResponse
                {
                    Id = s.Id, Name = s.Name, Description = s.Description,
                    IsCustom = s.IsCustom, Gender = s.Gender.Name
                }));
            });

    internal static Delegate CreateSizeHandler =>
        async (CreateSizeRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateSizeRequest, DictionaryServices>(request, sp,
                async (services) =>
                {
                    var size = new Size
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = request.Name,
                        Description = request.Description ?? string.Empty,
                        IsCustom = request.IsCustom,
                        Gender = ProductSizeGender.FromId<ProductSizeGender>(request.GenderId) ?? ProductSizeGender.Unisex
                    };
                    await services.DbContext.Sizes.AddAsync(size);
                    await services.DbContext.SaveChangesAsync();
                    return Results.Created($"/api/sizes/{size.Id}", new SizeResponse
                    {
                        Id = size.Id, Name = size.Name, Description = size.Description,
                        IsCustom = size.IsCustom, Gender = size.Gender.Name
                    });
                });

    internal static Delegate UpdateSizeHandler =>
        async (string id, CreateSizeRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateSizeRequest, DictionaryServices>(request, sp,
                async (services) =>
                {
                    var size = await services.DbContext.Sizes.FindAsync(id);
                    if (size == null) return Results.NotFound(RESTResult.Fail("Size not found."));
                    size.Name = request.Name;
                    size.Description = request.Description ?? string.Empty;
                    size.IsCustom = request.IsCustom;
                    size.Gender = ProductSizeGender.FromId<ProductSizeGender>(request.GenderId) ?? ProductSizeGender.Unisex;
                    await services.DbContext.SaveChangesAsync();
                    return Results.Ok(new SizeResponse { Id = size.Id, Name = size.Name, Description = size.Description, IsCustom = size.IsCustom, Gender = size.Gender.Name });
                });

    internal static Delegate DeleteSizeHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var deleted = await services.DbContext.Sizes.Where(s => s.Id == id).ExecuteDeleteAsync();
                return deleted > 0 ? Results.NoContent() : Results.NotFound(RESTResult.Fail("Size not found."));
            });

    // === COLORS ===
    internal static Delegate GetColorsHandler =>
        async (IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var colors = await services.DbContext.Colors.AsNoTracking().ToListAsync();
                return Results.Ok(colors.Select(c => new ColorResponse { Id = c.Id, Name = c.Name, HexCode = c.HexCode }));
            });

    internal static Delegate CreateColorHandler =>
        async (CreateColorRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateColorRequest, DictionaryServices>(request, sp,
                async (services) =>
                {
                    var color = new Color { Id = Guid.NewGuid().ToString(), Name = request.Name, HexCode = request.HexCode };
                    await services.DbContext.Colors.AddAsync(color);
                    await services.DbContext.SaveChangesAsync();
                    return Results.Created($"/api/colors/{color.Id}", new ColorResponse { Id = color.Id, Name = color.Name, HexCode = color.HexCode });
                });

    internal static Delegate UpdateColorHandler =>
        async (string id, CreateColorRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateColorRequest, DictionaryServices>(request, sp,
                async (services) =>
                {
                    var color = await services.DbContext.Colors.FindAsync(id);
                    if (color == null) return Results.NotFound(RESTResult.Fail("Color not found."));
                    color.Name = request.Name;
                    color.HexCode = request.HexCode;
                    await services.DbContext.SaveChangesAsync();
                    return Results.Ok(new ColorResponse { Id = color.Id, Name = color.Name, HexCode = color.HexCode });
                });

    internal static Delegate DeleteColorHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var deleted = await services.DbContext.Colors.Where(c => c.Id == id).ExecuteDeleteAsync();
                return deleted > 0 ? Results.NoContent() : Results.NotFound(RESTResult.Fail("Color not found."));
            });

    // === MATERIALS ===
    internal static Delegate GetMaterialsHandler =>
        async (IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var materials = await services.DbContext.Materials.AsNoTracking().ToListAsync();
                return Results.Ok(materials.Select(m => new MaterialResponse { Id = m.Id, Name = m.Name, Description = m.Description, ImageUrl = m.ImageUrl }));
            });

    internal static Delegate CreateMaterialHandler =>
        async (CreateMaterialRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateMaterialRequest, DictionaryServices>(request, sp,
                async (services) =>
                {
                    var material = new Material { Id = Guid.NewGuid().ToString(), Name = request.Name, Description = request.Description ?? string.Empty, ImageUrl = request.ImageUrl };
                    await services.DbContext.Materials.AddAsync(material);
                    await services.DbContext.SaveChangesAsync();
                    return Results.Created($"/api/materials/{material.Id}", new MaterialResponse { Id = material.Id, Name = material.Name, Description = material.Description, ImageUrl = material.ImageUrl });
                });

    internal static Delegate UpdateMaterialHandler =>
        async (string id, CreateMaterialRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateMaterialRequest, DictionaryServices>(request, sp,
                async (services) =>
                {
                    var material = await services.DbContext.Materials.FindAsync(id);
                    if (material == null) return Results.NotFound(RESTResult.Fail("Material not found."));
                    material.Name = request.Name;
                    material.Description = request.Description ?? string.Empty;
                    material.ImageUrl = request.ImageUrl;
                    await services.DbContext.SaveChangesAsync();
                    return Results.Ok(new MaterialResponse { Id = material.Id, Name = material.Name, Description = material.Description, ImageUrl = material.ImageUrl });
                });

    internal static Delegate DeleteMaterialHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<DictionaryServices>(sp, async (services) =>
            {
                var deleted = await services.DbContext.Materials.Where(m => m.Id == id).ExecuteDeleteAsync();
                return deleted > 0 ? Results.NoContent() : Results.NotFound(RESTResult.Fail("Material not found."));
            });
}
