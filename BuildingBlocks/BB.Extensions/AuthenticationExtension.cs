using Microsoft.Extensions.DependencyInjection;

namespace BB.Extensions;

public static class AuthenticationExtension
{
    public static IServiceCollection AddCommonAuthenticationApi(this IServiceCollection services)
    {
        services.AddAuthentication(options => { })
                .AddJwtBearer(options => { });
        
        services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        
        return services;
    }
}
