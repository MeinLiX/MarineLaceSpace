using BB.Common.Routes;
using MarineLaceSpace.DTO.Requests.Auth;
using MarineLaceSpace.Models.Routes;
using Microsoft.AspNetCore.Mvc;

namespace Auth.WebHost.Routes;

internal class AuthHandlers
{
    private record AuthServices : BasicRouteServices
    {
        public required ILogger<AuthHandlers> Logger { get; init; }
    }

    internal static Delegate LoginRouteHandler =>
        async ([FromBody] LoginDto loginDto,
               [FromServices] IServiceProvider serviceProvider) =>
                await RouteHandlers.RouteHandlerAsync<LoginDto, AuthServices>(loginDto, serviceProvider,
                (services) =>
                {
                    services.Logger.LogInformation("Login!");

                    return Task.FromResult(Results.Ok(loginDto.Email));
                });


    internal static Delegate RegisterRouteHandler =>
        ([FromBody] RegisterDto registerDto, CancellationToken cancellationToken) =>
        {
            return Results.Ok();
        };

    internal static Delegate RefreshTokenRouteHandler =>
        ([FromBody] RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken) =>
        {
            return Results.Ok();
        };
}
