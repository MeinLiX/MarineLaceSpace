using MarineLaceSpace.Models.Database.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BB.Common.Data.DBContexts;

public abstract class AuthDbContext(DbContextOptions options) : IdentityDbContext<AuthUser>(options)
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder) => base.OnModelCreating(builder);
}
