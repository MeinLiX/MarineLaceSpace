using MarineLaceSpace.DTO.Requests.Catalog;
using MarineLaceSpace.Models.Database.Catalog;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByShopIdAsync(string shopId);
    Task<Product> GetByIdWithDetailsAsync(string id);
    Task<IEnumerable<Product>> GetByCategoryIdAsync(string categoryId, bool includeSubcategories = true);
    Task<decimal> GetProductPriceAsync(string productId, string? sizeId = null, string? materialId = null, string? colorId = null);
    Task UpdateInventoryAsync(string productId, IEnumerable<ProductPrice> newInventory);
    Task AssociateImagesWithVariationsAsync(string productId, IEnumerable<VariationImageAssociationRequest> associations);
}
