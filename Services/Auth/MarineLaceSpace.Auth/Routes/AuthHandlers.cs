using Auth.WebHost.Services;
using BB.Common.Routes;
using MarineLaceSpace.DTO.Requests.Auth;
using MarineLaceSpace.DTO.Responses;
using MarineLaceSpace.DTO.Responses.Auth;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.EventBus;
using MarineLaceSpace.Interfaces.Repositories.Auth;
using MarineLaceSpace.Models.Database.Auth;
using MarineLaceSpace.Models.Events;
using MarineLaceSpace.Models.Routes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.WebHost.Routes;

internal class AuthHandlers
{
    private record AuthServices : BasicRouteServices
    {
        public required IAuthUserRepository AuthUserRepository { get; init; }
        public required IRefreshTokenRepository RefreshTokenRepository { get; init; }
        public required UserManager<AuthUser> UserManager { get; init; }
        public required SignInManager<AuthUser> SignInManager { get; init; }
        public required JwtTokenService JwtTokenService { get; init; }
        public required ILogger<AuthHandlers> Logger { get; init; }
        public IEventBus? EventBus { get; init; }
    }

    internal static Delegate LoginRouteHandler =>
        async ([FromBody] LoginDto loginDto,
               [FromServices] IServiceProvider serviceProvider) =>
            await RouteHandlers.RouteHandlerAsync<LoginDto, AuthServices>(loginDto, serviceProvider,
                async (services) =>
                {
                    try
                    {
                        var user = await services.AuthUserRepository.GetByEmailAsync(loginDto.Email);

                        if (user.IsAnonimous)
                        {
                            return Results.BadRequest(RESTResult.Fail("Invalid credentials."));
                        }

                        var signInResult = await services.SignInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: true);

                        if (signInResult.IsLockedOut)
                        {
                            return Results.BadRequest(RESTResult.Fail("Account is temporarily locked. Try again later."));
                        }

                        if (!signInResult.Succeeded)
                        {
                            return Results.BadRequest(RESTResult.Fail("Invalid credentials."));
                        }

                        var (accessToken, accessExpiresAt) = await services.JwtTokenService.GenerateAccessTokenAsync(user);
                        var refreshToken = services.JwtTokenService.GenerateRefreshToken(user.Id);

                        await services.RefreshTokenRepository.AddAsync(refreshToken);

                        services.Logger.LogInformation("User {UserId} logged in successfully", user.Id);

                        return Results.Ok(RESTResult<AuthResponseDto>.Success(new AuthResponseDto
                        {
                            UserId = user.Id,
                            AccessToken = accessToken,
                            RefreshToken = refreshToken.Token,
                            AccessTokenExpiresAt = accessExpiresAt,
                            RefreshTokenExpiresAt = refreshToken.ExpiryDate
                        }));
                    }
                    catch (NotFoundEntityException)
                    {
                        return Results.BadRequest(RESTResult.Fail("Invalid credentials."));
                    }
                });

    internal static Delegate RegisterRouteHandler =>
        async ([FromBody] RegisterDto registerDto,
               [FromServices] IServiceProvider serviceProvider) =>
            await RouteHandlers.RouteHandlerAsync<RegisterDto, AuthServices>(registerDto, serviceProvider,
                async (services) =>
                {
                    try
                    {
                        var newUser = new AuthUser
                        {
                            Id = Guid.NewGuid().ToString(),
                            Email = registerDto.Email,
                            UserName = registerDto.Email,
                            EmailConfirmed = true,
                            FirstName = registerDto.FirstName,
                            LastName = registerDto.LastName
                        };

                        if (!registerDto.IsAnonumous)
                        {
                            var result = await services.UserManager.CreateAsync(newUser, registerDto.Password!);
                            if (!result.Succeeded)
                            {
                                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                                return Results.BadRequest(RESTResult.Fail(errors));
                            }
                            await services.UserManager.AddToRoleAsync(newUser, "Customer");
                        }
                        else
                        {
                            var result = await services.UserManager.CreateAsync(newUser);
                            if (!result.Succeeded)
                            {
                                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                                return Results.BadRequest(RESTResult.Fail(errors));
                            }
                            await services.UserManager.AddToRoleAsync(newUser, "Anonimous");
                        }

                        var (accessToken, accessExpiresAt) = await services.JwtTokenService.GenerateAccessTokenAsync(newUser);
                        var refreshToken = services.JwtTokenService.GenerateRefreshToken(newUser.Id, Guid.NewGuid().ToString());
                        await services.RefreshTokenRepository.AddAsync(refreshToken);

                        if (services.EventBus != null)
                        {
                            await services.EventBus.PublishAsync(new UserRegisteredEvent
                            {
                                UserId = newUser.Id,
                                Email = newUser.Email!,
                                IsAnonymous = registerDto.IsAnonumous
                            });
                        }

                        services.Logger.LogInformation("User {UserId} registered (anonymous: {IsAnonymous})", newUser.Id, registerDto.IsAnonumous);

                        return Results.Ok(RESTResult<AuthResponseDto>.Success(new AuthResponseDto
                        {
                            UserId = newUser.Id,
                            AccessToken = accessToken,
                            RefreshToken = refreshToken.Token,
                            AccessTokenExpiresAt = accessExpiresAt,
                            RefreshTokenExpiresAt = refreshToken.ExpiryDate
                        }));
                    }
                    catch (Exception ex)
                    {
                        services.Logger.LogError(ex, "Registration failed for {Email}", registerDto.Email);
                        return Results.BadRequest(RESTResult.Fail("Registration failed."));
                    }
                });

    internal static Delegate RefreshTokenRouteHandler =>
        async ([FromBody] RefreshTokenDto refreshTokenDto,
               [FromServices] IServiceProvider serviceProvider) =>
            await RouteHandlers.RouteHandlerAsync<RefreshTokenDto, AuthServices>(refreshTokenDto, serviceProvider,
                async (services) =>
                {
                    try
                    {
                        var existingToken = await services.RefreshTokenRepository.GetByTokenAsync(refreshTokenDto.RefreshToken);

                        if (existingToken.IsRevoked)
                        {
                            services.Logger.LogWarning("Token reuse detected for family {TokenFamily}, user {UserId}", existingToken.TokenFamily, existingToken.UserId);
                            await services.RefreshTokenRepository.RevokeTokenFamilyAsync(existingToken.TokenFamily);
                            return Results.Json(RESTResult.Fail("Token reuse detected. All sessions have been revoked."), statusCode: StatusCodes.Status401Unauthorized);
                        }

                        if (existingToken.ExpiryDate < DateTime.UtcNow)
                        {
                            return Results.BadRequest(RESTResult.Fail("Invalid or expired refresh token."));
                        }

                        var user = existingToken.User ?? await services.AuthUserRepository.GetByIdAsync(existingToken.UserId);

                        await services.RefreshTokenRepository.DeleteAsync(existingToken.Id);

                        var (accessToken, accessExpiresAt) = await services.JwtTokenService.GenerateAccessTokenAsync(user);
                        var newRefreshToken = services.JwtTokenService.GenerateRefreshToken(user.Id, existingToken.TokenFamily);
                        await services.RefreshTokenRepository.AddAsync(newRefreshToken);

                        return Results.Ok(RESTResult<RefreshTokenResponseDto>.Success(new RefreshTokenResponseDto
                        {
                            AccessToken = accessToken,
                            RefreshToken = newRefreshToken.Token,
                            AccessTokenExpiresAt = accessExpiresAt,
                            RefreshTokenExpiresAt = newRefreshToken.ExpiryDate
                        }));
                    }
                    catch (NotFoundEntityException)
                    {
                        return Results.BadRequest(RESTResult.Fail("Invalid refresh token."));
                    }
                });

    internal static Delegate GetCurrentUserHandler =>
        async ([FromServices] IServiceProvider serviceProvider) =>
            await RouteHandlers.RouteHandlerAsync<AuthServices>(serviceProvider,
                async (services) =>
                {
                    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                    var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(userId))
                        return Results.Unauthorized();

                    try
                    {
                        var user = await services.AuthUserRepository.GetByIdAsync(userId);
                        var roles = await services.UserManager.GetRolesAsync(user);
                        return Results.Ok(new
                        {
                            user.Id,
                            user.Email,
                            user.IsAnonimous,
                            Roles = roles,
                            user.CreatedAt
                        });
                    }
                    catch (NotFoundEntityException)
                    {
                        return Results.NotFound(RESTResult.Fail("User not found."));
                    }
                });

    internal static Delegate ForgotPasswordHandler =>
        async ([FromBody] ForgotPasswordDto dto,
               [FromServices] IServiceProvider serviceProvider) =>
            await RouteHandlers.RouteHandlerAsync<AuthServices>(serviceProvider,
                async (services) =>
                {
                    try
                    {
                        var user = await services.AuthUserRepository.GetByEmailAsync(dto.Email);
                        var resetToken = await services.UserManager.GeneratePasswordResetTokenAsync(user);

                        if (services.EventBus != null)
                        {
                            await services.EventBus.PublishAsync(new PasswordResetRequestedEvent
                            {
                                UserId = user.Id,
                                Email = user.Email!,
                                ResetToken = resetToken
                            });
                        }
                    }
                    catch (NotFoundEntityException)
                    {
                        // Don't reveal that user doesn't exist
                    }

                    return Results.Ok(RESTResult.Success("If the email exists, a reset link has been sent."));
                });

    internal static Delegate ResetPasswordHandler =>
        async ([FromBody] ResetPasswordDto dto,
               [FromServices] IServiceProvider serviceProvider) =>
            await RouteHandlers.RouteHandlerAsync<AuthServices>(serviceProvider,
                async (services) =>
                {
                    try
                    {
                        var user = await services.AuthUserRepository.GetByEmailAsync(dto.Email);
                        var result = await services.UserManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

                        if (!result.Succeeded)
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            return Results.BadRequest(RESTResult.Fail(errors));
                        }

                        return Results.Ok(RESTResult.Success("Password has been reset."));
                    }
                    catch (NotFoundEntityException)
                    {
                        return Results.BadRequest(RESTResult.Fail("Invalid reset request."));
                    }
                });

    internal static Delegate AssignRoleHandler =>
        async (string userId, [FromBody] AssignRoleDto dto,
               [FromServices] IServiceProvider serviceProvider) =>
            await RouteHandlers.RouteHandlerAsync<AuthServices>(serviceProvider,
                async (services) =>
                {
                    try
                    {
                        await services.AuthUserRepository.AddToRolesAsync(userId, dto.Roles);
                        return Results.Ok(RESTResult.Success("Roles assigned."));
                    }
                    catch (NotFoundEntityException)
                    {
                        return Results.NotFound(RESTResult.Fail("User not found."));
                    }
                });

    internal static Delegate GetUserByIdHandler =>
        async (string id, [FromServices] IServiceProvider serviceProvider) =>
            await RouteHandlers.RouteHandlerAsync<AuthServices>(serviceProvider,
                async (services) =>
                {
                    try
                    {
                        var user = await services.AuthUserRepository.GetByIdAsync(id);
                        var roles = await services.UserManager.GetRolesAsync(user);

                        return Results.Ok(RESTResult<UserProfileResponse>.Success(new UserProfileResponse
                        {
                            Id = user.Id,
                            Email = user.Email ?? string.Empty,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            PhoneNumber = user.PhoneNumber,
                            IsAnonymous = user.IsAnonimous,
                            CreatedAt = user.CreatedAt,
                            Roles = roles.ToList()
                        }));
                    }
                    catch (NotFoundEntityException)
                    {
                        return Results.NotFound(RESTResult.Fail("User not found."));
                    }
                });

    internal static Delegate UpdateCurrentUserProfileHandler =>
        async ([FromBody] UpdateUserProfileDto dto,
               [FromServices] IServiceProvider serviceProvider) =>
            await RouteHandlers.RouteHandlerAsync<UpdateUserProfileDto, AuthServices>(dto, serviceProvider,
                async (services) =>
                {
                    try
                    {
                        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (string.IsNullOrEmpty(userId))
                            return Results.Unauthorized();

                        var user = await services.AuthUserRepository.GetByIdAsync(userId);

                        if (dto.FirstName != null) user.FirstName = dto.FirstName;
                        if (dto.LastName != null) user.LastName = dto.LastName;
                        if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;

                        await services.AuthUserRepository.UpdateAsync(user);
                        var roles = await services.UserManager.GetRolesAsync(user);

                        return Results.Ok(RESTResult<UserProfileResponse>.Success(new UserProfileResponse
                        {
                            Id = user.Id,
                            Email = user.Email ?? string.Empty,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            PhoneNumber = user.PhoneNumber,
                            IsAnonymous = user.IsAnonimous,
                            CreatedAt = user.CreatedAt,
                            Roles = roles.ToList()
                        }));
                    }
                    catch (NotFoundEntityException)
                    {
                        return Results.NotFound(RESTResult.Fail("User not found."));
                    }
                    catch (Exception ex)
                    {
                        return Results.BadRequest(RESTResult.Fail(ex.Message));
                    }
                });

    internal static Delegate ChangePasswordHandler =>
        async ([FromBody] ChangePasswordDto dto,
               [FromServices] IServiceProvider serviceProvider) =>
            await RouteHandlers.RouteHandlerAsync<ChangePasswordDto, AuthServices>(dto, serviceProvider,
                async (services) =>
                {
                    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                    var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(userId))
                        return Results.Unauthorized();

                    try
                    {
                        var user = await services.AuthUserRepository.GetByIdAsync(userId);

                        var result = await services.UserManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
                        if (!result.Succeeded)
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            return Results.BadRequest(RESTResult.Fail(errors));
                        }

                        await services.RefreshTokenRepository.RevokeAllUserTokensAsync(userId);

                        services.Logger.LogInformation("User {UserId} changed password successfully", userId);

                        return Results.Ok(RESTResult.Success("Password changed successfully. Please log in again."));
                    }
                    catch (NotFoundEntityException)
                    {
                        return Results.NotFound(RESTResult.Fail("User not found."));
                    }
                });

    internal static Delegate LogoutHandler =>
        async ([FromServices] IServiceProvider serviceProvider) =>
            await RouteHandlers.RouteHandlerAsync<AuthServices>(serviceProvider,
                async (services) =>
                {
                    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                    var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(userId))
                        return Results.Unauthorized();

                    await services.RefreshTokenRepository.RevokeAllUserTokensAsync(userId);

                    services.Logger.LogInformation("User {UserId} logged out, all refresh tokens revoked", userId);

                    return Results.Ok(RESTResult.Success("Logged out successfully."));
                });
}
