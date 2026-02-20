namespace Catalog.WebHost.Routes;

public static class DictionaryRoutes
{
    public static void MapDictionaryRoutes(this IEndpointRouteBuilder app)
    {
        // Sizes
        var sizesGroup = app.MapGroup("/api/sizes").WithTags("Sizes");
        sizesGroup.MapGet("/", DictionaryHandlers.GetSizesHandler).WithSummary("Get all sizes");
        sizesGroup.MapPost("/", DictionaryHandlers.CreateSizeHandler).WithSummary("Create a size").RequireAuthorization("AdminOnly");
        sizesGroup.MapPut("/{id}", DictionaryHandlers.UpdateSizeHandler).WithSummary("Update a size").RequireAuthorization("AdminOnly");
        sizesGroup.MapDelete("/{id}", DictionaryHandlers.DeleteSizeHandler).WithSummary("Delete a size").RequireAuthorization("AdminOnly");

        // Colors
        var colorsGroup = app.MapGroup("/api/colors").WithTags("Colors");
        colorsGroup.MapGet("/", DictionaryHandlers.GetColorsHandler).WithSummary("Get all colors");
        colorsGroup.MapPost("/", DictionaryHandlers.CreateColorHandler).WithSummary("Create a color").RequireAuthorization("AdminOnly");
        colorsGroup.MapPut("/{id}", DictionaryHandlers.UpdateColorHandler).WithSummary("Update a color").RequireAuthorization("AdminOnly");
        colorsGroup.MapDelete("/{id}", DictionaryHandlers.DeleteColorHandler).WithSummary("Delete a color").RequireAuthorization("AdminOnly");

        // Materials
        var materialsGroup = app.MapGroup("/api/materials").WithTags("Materials");
        materialsGroup.MapGet("/", DictionaryHandlers.GetMaterialsHandler).WithSummary("Get all materials");
        materialsGroup.MapPost("/", DictionaryHandlers.CreateMaterialHandler).WithSummary("Create a material").RequireAuthorization("AdminOnly");
        materialsGroup.MapPut("/{id}", DictionaryHandlers.UpdateMaterialHandler).WithSummary("Update a material").RequireAuthorization("AdminOnly");
        materialsGroup.MapDelete("/{id}", DictionaryHandlers.DeleteMaterialHandler).WithSummary("Delete a material").RequireAuthorization("AdminOnly");
    }
}
