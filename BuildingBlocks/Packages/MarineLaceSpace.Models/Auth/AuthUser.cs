using Microsoft.AspNetCore.Identity;

namespace MarineLaceSpace.Models.Auth;

public class AuthUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}


