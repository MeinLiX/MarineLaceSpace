using MarineLaceSpace.Models.Auth;

namespace MarineLaceSpace.Interfaces.Repositories.Auth;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken> GetByTokenAsync(string token, bool includeUser = true);
    Task DeleteExpiredTokensAsync();
}
