using MarineLaceSpace.Catalog.Data.DBContexts;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using MarineLaceSpace.Models.Database.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MarineLaceSpace.Catalog.Data.Repositories;

public class ProductPhotoRepository(CatalogDbContext context) : IProductPhotoRepository
{
    private readonly CatalogDbContext _context = context;

    public async Task<ProductPhoto> GetByIdAsync(string entityId)
    {
        return await _context.ProductPhotos.FindAsync(entityId)
               ?? throw new NotFoundEntityException($"Photo with ID '{entityId}' not found.");
    }

    public async Task<IEnumerable<ProductPhoto>> GetByProductIdAsync(string productId)
    {
        return await _context.ProductPhotos
            .Where(p => p.ProductId == productId)
            .OrderBy(p => p.SortOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductPhoto>> GetByProductVariantAsync(string productId, string? sizeId = null, string? materialId = null, string? colorId = null)
    {
        return await _context.ProductPhotos
            .Where(p => p.ProductId == productId &&
                        p.ProductSizeId == sizeId &&
                        p.ProductMaterialId == materialId &&
                        p.ProductColorId == colorId)
            .OrderBy(p => p.SortOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ProductPhoto> GetMainPhotoAsync(string productId)
    {
        return await _context.ProductPhotos
                   .AsNoTracking()
                   .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsMain)
               ?? await _context.ProductPhotos
                   .AsNoTracking()
                   .OrderBy(p => p.SortOrder)
                   .FirstOrDefaultAsync(p => p.ProductId == productId)
               ?? throw new NotFoundEntityException($"No photos found for product '{productId}'.");
    }

    public async Task<ProductPhoto> AddAsync(ProductPhoto entity)
    {
        await _context.ProductPhotos.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(ProductPhoto entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string entityId)
    {
        var deleted = await _context.ProductPhotos
            .Where(p => p.Id == entityId)
            .ExecuteDeleteAsync();

        if (deleted == 0)
            throw new NotFoundEntityException($"Photo with ID '{entityId}' not found for deletion.");
    }
}
