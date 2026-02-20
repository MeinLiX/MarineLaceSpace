using MarineLaceSpace.Catalog.Data.DBContexts;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using MarineLaceSpace.Models.Database.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MarineLaceSpace.Catalog.Data.Repositories;

public class ProductReviewRepository(CatalogDbContext context) : IProductReviewRepository
{
    private readonly CatalogDbContext _context = context;

    public async Task<ProductReview> GetByIdAsync(string entityId)
    {
        return await _context.ProductReviews.FindAsync(entityId)
               ?? throw new NotFoundEntityException($"Review with ID '{entityId}' not found.");
    }

    public async Task<IEnumerable<ProductReview>> GetByProductIdAsync(string productId)
    {
        return await _context.ProductReviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<decimal> GetAverageRatingAsync(string productId)
    {
        var reviews = await _context.ProductReviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        if (reviews.Count == 0) return 0;
        return reviews.Average(r => r.Rating);
    }

    public async Task<ProductReview> AddAsync(ProductReview entity)
    {
        await _context.ProductReviews.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(ProductReview entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string entityId)
    {
        var deleted = await _context.ProductReviews
            .Where(r => r.Id == entityId)
            .ExecuteDeleteAsync();

        if (deleted == 0)
            throw new NotFoundEntityException($"Review with ID '{entityId}' not found for deletion.");
    }
}
