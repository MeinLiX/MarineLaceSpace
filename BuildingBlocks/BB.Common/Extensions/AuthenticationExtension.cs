using MarineLaceSpace.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BB.Common.Extensions;

public static class AuthenticationExtension
{
    public static IServiceCollection AddCommonAuthenticationApi(this IServiceCollection services)
    {
        services
            .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                var jwtSettings = services.BuildServiceProvider().GetRequiredService<JwtSettingsOption>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidIssuer = jwtSettings.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtSettings.SecretKeyBase64)),
                    ClockSkew = TimeSpan.Zero
                };
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
            });

        services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

        return services;
    }
}
