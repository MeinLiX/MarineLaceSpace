using MarineLaceSpace.Models.Database.Auth;

namespace MarineLaceSpace.Interfaces.Repositories.Auth;

public interface IAuthUserRepository : IRepository<AuthUser, string>
{
    Task<AuthUser> GetByEmailAsync(string email);
    Task<AuthUser> GetByRefreshTokenAsync(string token);
    Task AddToRolesAsync(string entityId, params string[] roles);
}
