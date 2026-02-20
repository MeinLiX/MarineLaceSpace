using BB.Common.Routes;
using MarineLaceSpace.DTO.Requests.Catalog;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using MarineLaceSpace.Models.Database.Catalog;
using MarineLaceSpace.Models.Routes;

namespace Catalog.WebHost.Routes;

internal class CategoryHandlers
{
    private record CategoryServices : BasicRouteServices
    {
        public required ICategoryRepository CategoryRepository { get; init; }
        public required ILogger<CategoryHandlers> Logger { get; init; }
    }

    internal static Delegate GetCategoryTreeHandler =>
        async (IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CategoryServices>(sp, async (services) =>
            {
                var rootCategories = await services.CategoryRepository.GetByParentIdAsync(null);
                var response = rootCategories.Select(MapCategoryToResponse).ToList();
                return Results.Ok(response);
            });

    internal static Delegate GetCategoryByIdHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CategoryServices>(sp, async (services) =>
            {
                try
                {
                    var category = await services.CategoryRepository.GetByIdAsync(id);
                    return Results.Ok(MapCategoryToResponse(category));
                }
                catch (NotFoundEntityException ex)
                {
                    return Results.NotFound(RESTResult.Fail(ex.Message));
                }
            });

    internal static Delegate GetCategoryPathHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CategoryServices>(sp, async (services) =>
            {
                var path = await services.CategoryRepository.GetCategoryPathAsync(id);
                var response = path.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ParentCategoryId = c.ParentCategoryId,
                    Level = c.Level,
                    FullPath = c.FullPath
                });
                return Results.Ok(response);
            });

    internal static Delegate CreateCategoryHandler =>
        async (CreateCategoryRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CreateCategoryRequest, CategoryServices>(request, sp,
                async (services) =>
                {
                    var newCategory = new Category
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = request.Name,
                        Description = request.Description,
                        ParentCategoryId = request.ParentCategoryId
                    };

                    var created = await services.CategoryRepository.AddAsync(newCategory);
                    return Results.Created($"/api/categories/{created.Id}", MapCategoryToResponse(created));
                });

    internal static Delegate UpdateCategoryHandler =>
        async (string id, UpdateCategoryRequest request, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<UpdateCategoryRequest, CategoryServices>(request, sp,
                async (services) =>
                {
                    try
                    {
                        var category = await services.CategoryRepository.GetByIdAsync(id);
                        category.Name = request.Name;
                        category.Description = request.Description;
                        category.FullPath = category.ParentCategoryId != null
                            ? $"{(await services.CategoryRepository.GetFullPathAsync(category.ParentCategoryId))} > {request.Name}"
                            : request.Name;

                        await services.CategoryRepository.UpdateAsync(category);
                        return Results.Ok(MapCategoryToResponse(category));
                    }
                    catch (NotFoundEntityException ex)
                    {
                        return Results.NotFound(RESTResult.Fail(ex.Message));
                    }
                });

    internal static Delegate DeleteCategoryHandler =>
        async (string id, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<CategoryServices>(sp, async (services) =>
            {
                try
                {
                    await services.CategoryRepository.DeleteAsync(id);
                    return Results.NoContent();
                }
                catch (NotFoundEntityException ex)
                {
                    return Results.NotFound(RESTResult.Fail(ex.Message));
                }
                catch (ValidationEntityException ex)
                {
                    return Results.BadRequest(RESTResult.Fail(ex.Message));
                }
            });

    private static CategoryResponse MapCategoryToResponse(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        ParentCategoryId = category.ParentCategoryId,
        Level = category.Level,
        FullPath = category.FullPath,
        Subcategories = category.Subcategories?.Select(MapCategoryToResponse).ToList() ?? []
    };
}
