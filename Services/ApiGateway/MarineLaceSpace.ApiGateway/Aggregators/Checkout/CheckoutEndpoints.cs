namespace ApiGateway.WebHost.Aggregators.Checkout
{
    public static class CheckoutEndpoints
    {
        public static IEndpointRouteBuilder MapCheckoutAggregator(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/api/checkout", async (HttpContext context, ICheckoutAggregator aggregator) =>
            {
                var request = await context.Request.ReadFromJsonAsync<CheckoutRequestDto>();

                if (request == null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = "Invalid request data" });
                    return;
                }

                var result = await aggregator.ProcessCheckoutAsync(request);

                if (result.Success)
                {
                    await context.Response.WriteAsJsonAsync(result);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(result);
                }
            });

            return endpoints;
        }
    }
}