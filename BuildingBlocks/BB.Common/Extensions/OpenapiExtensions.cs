using APIWeaver;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace BB.Common.Extensions;

public static class OpenapiExtensions
{
    public static IServiceCollection AddCommonOpenApi(this IServiceCollection services)
           => services.AddOpenApi("api", options => {
               options.AddSecurityScheme("Bearer", scheme =>
               {
                   scheme.Description = "JWT Authorization";
                   scheme.Name = "Authorization";
                   scheme.In = ParameterLocation.Header;
                   scheme.Type = SecuritySchemeType.Http;
                   scheme.Scheme = "Bearer";
                   scheme.BearerFormat = "JWT";
               });

               options.AddAuthResponse();
           });

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
                   .WithHttpBearerAuthentication(bearerOptions => bearerOptions.Token = "");
        });
    }
}
