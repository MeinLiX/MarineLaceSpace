using BB.Common.Extensions;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Auth;

namespace Auth.WebHost.Routes;

public static class Routes
{
    public static WebApplication InitRoutes(this WebApplication web)
    {
        web.InitAuthRoutes();
        web.InitUserRoutes();
        return web;
    }

    private static RouteGroupBuilder InitAuthRoutes(this WebApplication web)
    {
        var authGroup = web.MapGroup("auth")
                           .WithTags("Auth");

        authGroup.MapPost("login", AuthHandlers.LoginRouteHandler)
                 .WithSummary("Login to the system")
                 .WithDescription("Authenticate with email and password, returns JWT tokens")
                 .Produces<IRESTResult<AuthResponseDto>>(StatusCodes.Status200OK)
                 .Produces<IRESTResult>(StatusCodes.Status400BadRequest)
                 .AddValidationResponseType();

        authGroup.MapPost("register", AuthHandlers.RegisterRouteHandler)
                 .WithSummary("Register in the system")
                 .WithDescription("Register a new user, optionally anonymous")
                 .Produces<IRESTResult<AuthResponseDto>>(StatusCodes.Status200OK)
                 .Produces<IRESTResult>(StatusCodes.Status400BadRequest)
                 .AddValidationResponseType();

        authGroup.MapPost("refresh-token", AuthHandlers.RefreshTokenRouteHandler)
                 .WithSummary("Refresh JWT token pair")
                 .Produces<IRESTResult<RefreshTokenResponseDto>>(StatusCodes.Status200OK)
                 .Produces<IRESTResult>(StatusCodes.Status400BadRequest)
                 .AddValidationResponseType();

        authGroup.MapPost("forgot-password", AuthHandlers.ForgotPasswordHandler)
                 .WithSummary("Request password reset")
                 .Produces<IRESTResult>(StatusCodes.Status200OK);

        authGroup.MapPost("reset-password", AuthHandlers.ResetPasswordHandler)
                 .WithSummary("Reset password with token")
                 .Produces<IRESTResult>(StatusCodes.Status200OK)
                 .Produces<IRESTResult>(StatusCodes.Status400BadRequest);

        return authGroup;
    }

    private static void InitUserRoutes(this WebApplication web)
    {
        var usersGroup = web.MapGroup("users")
                            .WithTags("Users");

        usersGroup.MapGet("me", AuthHandlers.GetCurrentUserHandler)
                  .RequireAuthorization()
                  .WithSummary("Get current user profile");

        usersGroup.MapPost("{userId}/roles", AuthHandlers.AssignRoleHandler)
                  .RequireAuthorization("AdminOnly")
                  .WithSummary("Assign roles to a user");
    }
}
