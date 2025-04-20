using MarineLaceSpace.Models.Database.Auth;

namespace MarineLaceSpace.Interfaces.Repositories.Auth;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken> GetByTokenAsync(string token, bool includeUser = true);
    Task DeleteExpiredTokensAsync();
}
