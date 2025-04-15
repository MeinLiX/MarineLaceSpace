using ApiGateway.WebHost.Aggregators.Checkout;

namespace ApiGateway.WebHost.Aggregators
{
    public static class AggregatorExtensions
    {
        public static IServiceCollection AddApiAggregators(this IServiceCollection services)
        {
            services.AddHttpClient("catalog-api", client => client.BaseAddress = new Uri("http://catalog-api"));
            services.AddHttpClient("basket-api", client => client.BaseAddress = new Uri("http://basket-api"));
            services.AddHttpClient("order-api", client => client.BaseAddress = new Uri("http://order-api"));
            services.AddHttpClient("payment-api", client => client.BaseAddress = new Uri("http://payment-api"));

            
            services.AddScoped<ICheckoutAggregator, CheckoutAggregator>();
            
            return services;
        }

        public static IEndpointRouteBuilder MapApiAggregators(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapCheckoutAggregator();

            return endpoints;
        }
    }
}