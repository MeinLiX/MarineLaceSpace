using MarineLaceSpace.Models.Database.Auth;
using MarineLaceSpace.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Auth.WebHost.Services;

public class JwtTokenService
{
    private readonly JwtSettingsOption _jwtSettings;
    private readonly UserManager<AuthUser> _userManager;

    public JwtTokenService(JwtSettingsOption jwtSettings, UserManager<AuthUser> userManager)
    {
        _jwtSettings = jwtSettings;
        _userManager = userManager;
    }

    public async Task<(string Token, DateTime ExpiresAt)> GenerateAccessTokenAsync(AuthUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Convert.FromBase64String(_jwtSettings.SecretKeyBase64));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public RefreshToken GenerateRefreshToken(string userId, string? tokenFamily = null)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid().ToString(),
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = userId,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            TokenFamily = tokenFamily ?? Guid.NewGuid().ToString()
        };
    }
}
