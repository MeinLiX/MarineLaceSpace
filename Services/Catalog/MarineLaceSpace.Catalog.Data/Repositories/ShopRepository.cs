using MarineLaceSpace.Catalog.Data.DBContexts;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using MarineLaceSpace.Models.Database.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MarineLaceSpace.Catalog.Data.Repositories;

public class ShopRepository(CatalogDbContext context) : IShopRepository
{
    private readonly CatalogDbContext _context = context;

    public async Task<Shop> GetByIdAsync(string entityId)
    {
        return await _context.Shops.FindAsync(entityId)
               ?? throw new NotFoundEntityException($"Shop with ID '{entityId}' not found.");
    }

    public async Task<Shop> GetBySlugAsync(string slug)
    {
        return await _context.Shops.AsNoTracking()
                   .FirstOrDefaultAsync(s => s.UrlSlug == slug)
               ?? throw new NotFoundEntityException($"Shop with slug '{slug}' not found.");
    }

    public async Task<Shop> GetByOwnerIdAsync(string ownerId)
    {
        return await _context.Shops.AsNoTracking()
                   .FirstOrDefaultAsync(s => s.OwnerId == ownerId)
               ?? throw new NotFoundEntityException($"Shop for owner with ID '{ownerId}' not found.");
    }

    public async Task<Shop> AddAsync(Shop entity)
    {
        await _context.Shops.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Shop entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string entityId)
    {
        var shopToDelete = await GetByIdAsync(entityId);
        _context.Shops.Remove(shopToDelete);
        await _context.SaveChangesAsync();
    }
}