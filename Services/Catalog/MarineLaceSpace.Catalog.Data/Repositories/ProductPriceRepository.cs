using MarineLaceSpace.Catalog.Data.DBContexts;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using MarineLaceSpace.Models.Database.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MarineLaceSpace.Catalog.Data.Repositories;

public class ProductPriceRepository(CatalogDbContext context) : IProductPriceRepository
{
    private readonly CatalogDbContext _context = context;

    public async Task<ProductPrice> GetByIdAsync(string entityId)
    {
        return await _context.ProductPrices.FindAsync(entityId)
               ?? throw new NotFoundEntityException($"ProductPrice with ID '{entityId}' not found.");
    }

    public async Task<ProductPrice> GetByVariantAsync(string productId, string? sizeId = null, string? materialId = null, string? colorId = null)
    {
        return await _context.ProductPrices
                   .AsNoTracking()
                   .FirstOrDefaultAsync(p =>
                       p.ProductId == productId &&
                       p.ProductSizeId == sizeId &&
                       p.ProductMaterialId == materialId &&
                       p.ProductColorId == colorId)
               ?? throw new NotFoundEntityException("Price for the specified variant not found.");
    }

    public async Task<decimal> GetPriceAsync(string productId, string? sizeId = null, string? materialId = null, string? colorId = null)
    {
        var price = await GetByVariantAsync(productId, sizeId, materialId, colorId);
        return price.BasePrice;
    }

    public async Task<decimal> GetBaseProductPriceAsync(string productId)
    {
        return await GetPriceAsync(productId);
    }

    public async Task<decimal> GetMinProductPriceAsync(string productId)
    {
        var hasAny = await _context.ProductPrices.AnyAsync(p => p.ProductId == productId);
        if (!hasAny) throw new NotFoundEntityException($"No prices found for product '{productId}'.");

        return await _context.ProductPrices
            .Where(p => p.ProductId == productId)
            .MinAsync(p => p.BasePrice);
    }

    public async Task<decimal> GetMaxProductPriceAsync(string productId)
    {
        var hasAny = await _context.ProductPrices.AnyAsync(p => p.ProductId == productId);
        if (!hasAny) throw new NotFoundEntityException($"No prices found for product '{productId}'.");

        return await _context.ProductPrices
            .Where(p => p.ProductId == productId)
            .MaxAsync(p => p.BasePrice);
    }

    public async Task<(decimal Min, decimal Max)> GetPriceRangeAsync(string productId)
    {
        var prices = await _context.ProductPrices
            .Where(p => p.ProductId == productId)
            .Select(p => p.BasePrice)
            .ToListAsync();

        if (prices.Count == 0) throw new NotFoundEntityException($"No prices found for product '{productId}'.");

        return (prices.Min(), prices.Max());
    }

    public async Task<ProductPrice> AddAsync(ProductPrice entity)
    {
        await _context.ProductPrices.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(ProductPrice entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string entityId)
    {
        var deleted = await _context.ProductPrices
            .Where(p => p.Id == entityId)
            .ExecuteDeleteAsync();

        if (deleted == 0)
            throw new NotFoundEntityException($"ProductPrice with ID '{entityId}' not found for deletion.");
    }
}
