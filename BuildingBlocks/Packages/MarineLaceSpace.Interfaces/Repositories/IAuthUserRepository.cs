using MarineLaceSpace.Models.Auth;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface IAuthUserRepository: IRepository<AuthUser>
{
    Task<AuthUser> GetByEmailAsync(string email);
    Task<AuthUser> GetByRefreshTokenAsync(string token);
}
