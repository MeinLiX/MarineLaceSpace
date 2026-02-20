using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;

namespace Catalog.WebHost.Routes;

public static class CategoryRoutes
{
    public static void MapCategoryRoutes(this IEndpointRouteBuilder app)
    {
        var categoriesGroup = app.MapGroup("/api/categories")
            .WithTags("Categories");

        categoriesGroup.MapGet("/tree", CategoryHandlers.GetCategoryTreeHandler)
            .WithSummary("Get full category tree");

        categoriesGroup.MapGet("/{id}", CategoryHandlers.GetCategoryByIdHandler)
            .WithSummary("Get category by ID with children")
            .Produces<CategoryResponse>()
            .Produces<IRESTResult>(StatusCodes.Status404NotFound);

        categoriesGroup.MapGet("/{id}/path", CategoryHandlers.GetCategoryPathHandler)
            .WithSummary("Get category breadcrumb path");

        categoriesGroup.MapPost("/", CategoryHandlers.CreateCategoryHandler)
            .WithSummary("Create a new category")
            .RequireAuthorization("AdminOnly");

        categoriesGroup.MapPut("/{id}", CategoryHandlers.UpdateCategoryHandler)
            .WithSummary("Update a category")
            .RequireAuthorization("AdminOnly");

        categoriesGroup.MapDelete("/{id}", CategoryHandlers.DeleteCategoryHandler)
            .WithSummary("Delete a category")
            .RequireAuthorization("AdminOnly");

        categoriesGroup.MapGet("/{id}/products", CategoryHandlers.GetCategoryProductsHandler)
            .WithSummary("Get products in a category")
            .Produces<IRESTResult>(StatusCodes.Status200OK)
            .Produces<IRESTResult>(StatusCodes.Status404NotFound);
    }
}
