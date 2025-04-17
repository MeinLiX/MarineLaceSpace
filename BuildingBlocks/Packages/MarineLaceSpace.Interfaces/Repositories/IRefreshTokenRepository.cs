using MarineLaceSpace.Models.Auth;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken> GetByTokenAsync(string token);
    Task DeleteExpiredTokensAsync();
}
