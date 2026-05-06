namespace Catalog.WebHost.Routes;

public static class DictionaryRoutes
{
    public static void MapDictionaryRoutes(this IEndpointRouteBuilder app)
    {
        var sizesGroup = app.MapGroup("/api/sizes").WithTags("Sizes");
        sizesGroup.MapGet("/", DictionaryHandlers.GetSizesHandler).WithSummary("Get sizes (filtered by scope)");
        sizesGroup.MapPost("/", DictionaryHandlers.CreateSizeHandler).WithSummary("Create a size").RequireAuthorization("SellersOrAdmin");
        sizesGroup.MapPut("/{id}", DictionaryHandlers.UpdateSizeHandler).WithSummary("Update a size").RequireAuthorization("SellersOrAdmin");
        sizesGroup.MapDelete("/{id}", DictionaryHandlers.DeleteSizeHandler).WithSummary("Delete a size").RequireAuthorization("SellersOrAdmin");

        var colorsGroup = app.MapGroup("/api/colors").WithTags("Colors");
        colorsGroup.MapGet("/", DictionaryHandlers.GetColorsHandler).WithSummary("Get colors (filtered by scope)");
        colorsGroup.MapPost("/", DictionaryHandlers.CreateColorHandler).WithSummary("Create a color").RequireAuthorization("SellersOrAdmin");
        colorsGroup.MapPut("/{id}", DictionaryHandlers.UpdateColorHandler).WithSummary("Update a color").RequireAuthorization("SellersOrAdmin");
        colorsGroup.MapDelete("/{id}", DictionaryHandlers.DeleteColorHandler).WithSummary("Delete a color").RequireAuthorization("SellersOrAdmin");

        var materialsGroup = app.MapGroup("/api/materials").WithTags("Materials");
        materialsGroup.MapGet("/", DictionaryHandlers.GetMaterialsHandler).WithSummary("Get materials (filtered by scope)");
        materialsGroup.MapPost("/", DictionaryHandlers.CreateMaterialHandler).WithSummary("Create a material").RequireAuthorization("SellersOrAdmin").DisableAntiforgery();
        materialsGroup.MapPut("/{id}", DictionaryHandlers.UpdateMaterialHandler).WithSummary("Update a material").RequireAuthorization("SellersOrAdmin").DisableAntiforgery();
        materialsGroup.MapDelete("/{id}", DictionaryHandlers.DeleteMaterialHandler).WithSummary("Delete a material").RequireAuthorization("SellersOrAdmin");
    }
}
