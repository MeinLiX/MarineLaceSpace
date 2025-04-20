using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace BB.Extensions;

public static class OpenapiExtensions
{
    public static IServiceCollection AddCommonOpenApi(this IServiceCollection services)
           => services.AddOpenApi("api", options => { });

    public static IEndpointConventionBuilder UseCommonScalar(this IEndpointRouteBuilder app, string title = "api")
    {
        app.MapOpenApi();

#if DEBUG
        app.MapGet("/", [ExcludeFromDescription] () => Results.Redirect("/scalar/api"));
#endif

        return app.MapScalarApiReference(options =>
        {
            options.WithTitle(title)
                   .WithTheme(ScalarTheme.Default)
                   .WithDarkMode(false)
                   .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

            options.Servers = Array.Empty<ScalarServer>();

            options.WithPreferredScheme("Bearer")
                   .WithHttpBearerAuthentication(bearerOptions => bearerOptions.Token = "put-here-your-token");
        });
    }
}
