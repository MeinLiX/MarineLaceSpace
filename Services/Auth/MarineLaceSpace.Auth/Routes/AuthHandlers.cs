using MarineLaceSpace.DTO.Requests.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Auth.WebHost.Routes;

internal class AuthHandlers
{
    internal static Delegate LoginRouteHandler =>
        ([FromBody] LoginDto loginDto, CancellationToken cancellationToken) =>
        {
            return Results.Ok();
        };

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
