namespace Catalog.WebHost.Routes;

public static class PriceRoutes
{
    public static void MapPriceRoutes(this IEndpointRouteBuilder app)
    {
        var priceGroup = app.MapGroup("/api/products/{productId}").WithTags("Pricing");
        priceGroup.MapGet("/price", PriceHandlers.GetPriceHandler).WithSummary("Get price for a specific variant");
        priceGroup.MapGet("/price-range", PriceHandlers.GetPriceRangeHandler).WithSummary("Get min-max price range");
    }
}
