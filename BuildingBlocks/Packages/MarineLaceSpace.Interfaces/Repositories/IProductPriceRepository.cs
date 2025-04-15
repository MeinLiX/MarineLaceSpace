using MarineLaceSpace.Models.Catalog;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface IProductPriceRepository : IRepository<ProductPrice>
{
    Task<ProductPrice> GetByVariantAsync(int productId, int? sizeId = null, int? materialId = null, int? colorId = null);
    Task<decimal> GetPriceAsync(int productId, int? sizeId = null, int? materialId = null, int? colorId = null);
    Task<decimal> GetBaseProductPriceAsync(int productId);
    Task<decimal> GetMinProductPriceAsync(int productId);
    Task<decimal> GetMaxProductPriceAsync(int productId);
    Task<(decimal Min, decimal Max)> GetPriceRangeAsync(int productId);
}
