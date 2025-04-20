using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Auth;

namespace Auth.WebHost.Routes;

public static class Routes
{
    public static WebApplication InitRoutes(this WebApplication web)
    {
        web.InitAuthRoutes();

        return web;
    }

    private static RouteGroupBuilder InitAuthRoutes(this WebApplication web)
    {
        var authGroup = web.MapGroup("auth")
                           .WithTags("auth");

        authGroup.MapPost("login", AuthHandlers.LoginRouteHandler)
                 .WithSummary("Login to the system")
                 .WithDescription("Generate JWT")
                 .Produces<IRestResponseResult<AuthResponseDto>>(StatusCodes.Status200OK)
                 .Produces<IErrorRestResponseResult<AuthResponseDto>>(StatusCodes.Status400BadRequest);


        authGroup.MapPost("register", AuthHandlers.RegisterRouteHandler)
                 .WithSummary("Register in the system")
                 .Produces<IRestResponseResult<AuthResponseDto>>(StatusCodes.Status200OK)
                 .Produces<IErrorRestResponseResult<AuthResponseDto>>(StatusCodes.Status400BadRequest);


        authGroup.MapPost("refresh-token", AuthHandlers.RefreshTokenRouteHandler)
                 .RequireAuthorization()
                 .WithSummary("Update JWT token")
                 .Produces<IRestResponseResult<RefreshTokenResponseDto>>(StatusCodes.Status200OK)
                 .Produces<IErrorRestResponseResult<RefreshTokenResponseDto>>(StatusCodes.Status400BadRequest);

        return authGroup;
    }
}
