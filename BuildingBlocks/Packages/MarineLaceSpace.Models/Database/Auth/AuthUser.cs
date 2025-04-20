using Microsoft.AspNetCore.Identity;

namespace MarineLaceSpace.Models.Database.Auth;

public class AuthUser : IdentityUser
{
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool IsAnonimous => string.IsNullOrWhiteSpace(PasswordHash);

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}


