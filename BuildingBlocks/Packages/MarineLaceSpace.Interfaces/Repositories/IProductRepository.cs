using MarineLaceSpace.Models.Catalog;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId, bool includeSubcategories = true);
    Task<decimal> GetProductPriceAsync(int productId, int? sizeId = null, int? materialId = null, int? colorId = null);
}
