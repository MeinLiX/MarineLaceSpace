using MarineLaceSpace.Models.Database.Catalog;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product> GetByIdWithDetailsAsync(string id);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(string categoryId, bool includeSubcategories = true);
    Task<decimal> GetProductPriceAsync(string productId, string? sizeId = null, string? materialId = null, string? colorId = null);
}
