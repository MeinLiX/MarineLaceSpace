using BB.Common.Routes;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Catalog;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using MarineLaceSpace.Models.Routes;

namespace Catalog.WebHost.Routes;

internal class PriceHandlers
{
    private record PriceServices : BasicRouteServices
    {
        public required IProductPriceRepository PriceRepository { get; init; }
        public required ILogger<PriceHandlers> Logger { get; init; }
    }

    internal static Delegate GetPriceHandler =>
        async (string productId, string? sizeId, string? colorId, string? materialId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<PriceServices>(sp, async (services) =>
            {
                try
                {
                    var price = await services.PriceRepository.GetPriceAsync(productId, sizeId, materialId, colorId);
                    return Results.Ok(new { Price = price });
                }
                catch (NotFoundEntityException ex)
                {
                    return Results.NotFound(RESTResult.Fail(ex.Message));
                }
            });

    internal static Delegate GetPriceRangeHandler =>
        async (string productId, IServiceProvider sp) =>
            await RouteHandlers.RouteHandlerAsync<PriceServices>(sp, async (services) =>
            {
                try
                {
                    var (min, max) = await services.PriceRepository.GetPriceRangeAsync(productId);
                    return Results.Ok(new PriceRangeResponse { MinPrice = min, MaxPrice = max });
                }
                catch (NotFoundEntityException ex)
                {
                    return Results.NotFound(RESTResult.Fail(ex.Message));
                }
            });
}
