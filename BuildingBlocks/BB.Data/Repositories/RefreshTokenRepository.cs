using BB.Data.DBContexts;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories.Auth;
using MarineLaceSpace.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace BB.Data.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _dbContext;

    public RefreshTokenRepository(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RefreshToken> AddAsync(RefreshToken entity)
    {
        var res = await _dbContext.RefreshTokens.AddAsync(entity);

        await _dbContext.SaveChangesAsync();

        return res.Entity;
    }

    public async Task DeleteAsync(string entityId)
    {
        var deleted = await _dbContext.RefreshTokens
                                      .Where(rt => rt.Id == entityId)
                                      .ExecuteDeleteAsync();

        if (deleted == 0)
            throw new NotFoundEntityException();
    }

    public async Task DeleteExpiredTokensAsync()
    {
        await _dbContext.RefreshTokens
                        .Where(rt => rt.ExpiryDate < DateTime.UtcNow)
                        .ExecuteDeleteAsync();
    }

    public async Task<RefreshToken> GetByIdAsync(string entityId)
    {
        return await _dbContext.RefreshTokens.FindAsync(entityId) ?? throw new NotFoundEntityException();
    }

    public async Task<RefreshToken> GetByTokenAsync(string token, bool includeUser = true)
    {
        var query = _dbContext.RefreshTokens.AsNoTracking();

        if (includeUser)
            query = query.Include(rt => rt.User);

        return await query.SingleOrDefaultAsync(rt => rt.Token == token)
               ?? throw new NotFoundEntityException();
    }

    public Task UpdateAsync(RefreshToken entity)
    {
        throw new NotSupportedEntityOperationException();
    }
}
