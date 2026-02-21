using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace BB.Common.Extensions;

public static class OpenapiExtensions
{
    public static IServiceCollection AddCommonOpenApi(this IServiceCollection services)
           => services.AddOpenApi("api", options => {
               options.AddDocumentTransformer((document, context, cancellationToken) =>
               {
                   var components = document.Components ??= new OpenApiComponents();
                   components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                   components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                   {
                       Description = "JWT Authorization",
                       In = ParameterLocation.Header,
                       Type = SecuritySchemeType.Http,
                       Scheme = "Bearer",
                       BearerFormat = "JWT"
                   };
                   return Task.CompletedTask;
               });

               options.AddOperationTransformer((operation, context, cancellationToken) =>
               {
                   var authAttributes = context.Description.ActionDescriptor.EndpointMetadata
                       .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
                   if (authAttributes.Any() && operation.Responses != null)
                   {
                       operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
                       operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
                   }
                   return Task.CompletedTask;
               });
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
                   .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

            options.Servers = Array.Empty<ScalarServer>();

            options.AddPreferredSecuritySchemes("Bearer")
                   .AddHttpAuthentication("Bearer", auth => auth.Token = "");
        });
    }
}
