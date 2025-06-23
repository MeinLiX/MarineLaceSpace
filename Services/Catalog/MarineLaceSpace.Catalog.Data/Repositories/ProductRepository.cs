using MarineLaceSpace.Catalog.Data.DBContexts;
using MarineLaceSpace.DTO.Requests.Catalog;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using MarineLaceSpace.Models.Database.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MarineLaceSpace.Catalog.Data.Repositories;

public class ProductRepository(CatalogDbContext context) : IProductRepository
{
    private readonly CatalogDbContext _context = context;

    public async Task<Product> AddAsync(Product entity)
    {
        await _context.Products.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Product> GetByIdAsync(string entityId)
    {
        return await _context.Products
                   .Include(p => p.Shop)
                   .Include(p => p.Photos)
                   .Include(p => p.ProductPrices)
                   .FirstOrDefaultAsync(p => p.Id == entityId)
               ?? throw new NotFoundEntityException($"Product with ID '{entityId}' not found.");
    }

    public async Task<IEnumerable<Product>> GetByShopIdAsync(string shopId)
    {
        return await _context.Products
            .Where(p => p.ShopId == shopId)
            .Include(p => p.Photos)
            .Include(p => p.ProductPrices)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task UpdateAsync(Product entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string entityId)
    {
        var affectedRows = await _context.Products
            .Where(p => p.Id == entityId)
            .ExecuteDeleteAsync();

        if (affectedRows == 0)
        {
            throw new NotFoundEntityException($"Product with ID '{entityId}' not found for deletion.");
        }
    }

    public async Task<Product> GetByIdWithDetailsAsync(string id)
    {
        return await _context.Products
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .Include(p => p.Photos)
            .Include(p => p.ProductPrices)
            .Include(p => p.AvailableSizes)
                .ThenInclude(ps => ps.Size)
            .Include(p => p.AvailableColors)
                .ThenInclude(pc => pc.Color)
            .Include(p => p.AvailableMaterials)
                .ThenInclude(pm => pm.Material)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundEntityException($"Product with ID '{id}' not found.");
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(string categoryId, bool includeSubcategories = true)
    {
        var categoryIds = new List<string> { categoryId };

        if (includeSubcategories)
        {
            var subCategoryIds = await GetAllSubcategoryIdsAsync(categoryId);
            categoryIds.AddRange(subCategoryIds);
        }

        return await _context.Products
            .Where(p => categoryIds.Contains(p.CategoryId) && p.IsActive)
            .Include(p => p.Photos)
            .Include(p => p.ProductPrices)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<decimal> GetProductPriceAsync(string productId, string? sizeId = null, string? materialId = null, string? colorId = null)
    {
        var priceEntry = await _context.ProductPrices
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.ProductId == productId &&
                p.ProductSizeId == sizeId &&
                p.ProductMaterialId == materialId &&
                p.ProductColorId == colorId)
            ?? throw new NotFoundEntityException($"Price for the specified product variation not found.");

        return priceEntry.BasePrice;
    }
    public async Task UpdateInventoryAsync(string productId, IEnumerable<ProductPrice> newInventory)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.ProductPrices.Where(p => p.ProductId == productId).ExecuteDeleteAsync();

            await _context.ProductPrices.AddRangeAsync(newInventory);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw; 
        }
    }
    public async Task AssociateImagesWithVariationsAsync(string productId, IEnumerable<VariationImageAssociationRequest> associations)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var photos = await _context.ProductPhotos
                .Where(p => p.ProductId == productId)
                .ToListAsync();

            if (!photos.Any())
            {
                await transaction.CommitAsync();
                return;
            }

            foreach (var photo in photos)
            {
                photo.ProductSizeId = null;
                photo.ProductColorId = null;
                photo.ProductMaterialId = null;
            }
            await _context.SaveChangesAsync();


            foreach (var association in associations)
            {
                var photoToUpdate = photos.FirstOrDefault(p => p.Id == association.ImageId);
                if (photoToUpdate != null)
                {
                    photoToUpdate.ProductSizeId = association.SizeId;
                    photoToUpdate.ProductColorId = association.ColorId;
                    photoToUpdate.ProductMaterialId = association.MaterialId;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    #region
    private async Task<List<string>> GetAllSubcategoryIdsAsync(string parentCategoryId)
    {
        var allIds = new List<string>();
        var subcategories = await _context.Categories
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var subId in subcategories)
        {
            allIds.Add(subId);
            allIds.AddRange(await GetAllSubcategoryIdsAsync(subId));
        }

        return allIds;
    }
    #endregion
}