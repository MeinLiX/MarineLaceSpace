using BB.Data.DBContexts;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories.Auth;
using MarineLaceSpace.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BB.Data.Repositories;

public class AuthUserRepository : IAuthUserRepository
{
    private readonly UserManager<AuthUser> _userManager;
    private readonly AuthDbContext _dbContext;

    public AuthUserRepository(UserManager<AuthUser> userManager, AuthDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<AuthUser> GetByIdAsync(string entityId)
    {
        return await _userManager.FindByIdAsync(entityId) ?? throw new NotFoundEntityException();
    }

    public async Task<AuthUser> GetByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) ?? throw new NotFoundEntityException();
    }

    public async Task<AuthUser> GetByRefreshTokenAsync(string token)
    {
        return await _dbContext.Users
                    .Include(u => u.RefreshTokens)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == token)) ?? throw new NotFoundEntityException();
    }

    public async Task<AuthUser> AddAsync(AuthUser entity)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            entity.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.CreateAsync(entity);
            if (!result.Succeeded)
            {
                throw new UserManagerException(result.Errors.Select(e => e.Code));
            }

            result = await _userManager.AddToRoleAsync(entity, "Anonimous");
            if (!result.Succeeded)
            {
                throw new UserManagerException(result.Errors.Select(e => e.Code));
            }

            var createdUser = await _userManager.FindByIdAsync(entity.Id) ?? throw new UserManagerException("User not found");

            await transaction.CommitAsync();

            return createdUser;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(AuthUser entity)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            entity.UpdatedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(entity);
            if (!result.Succeeded)
            {
                throw new UserManagerException(result.Errors.Select(e => e.Code));
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(string entityId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var userToDelete = await GetByIdAsync(entityId);
            var result = await _userManager.DeleteAsync(userToDelete);
            if (!result.Succeeded)
            {
                throw new UserManagerException(result.Errors.Select(e => e.Code));
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task AddToRolesAsync(string entityId, params string[] roles)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var userToUpdate = await GetByIdAsync(entityId);

            var result = await _userManager.AddToRolesAsync(userToUpdate, roles);
            if (!result.Succeeded)
            {
                throw new UserManagerException(result.Errors.Select(e => e.Code));
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
