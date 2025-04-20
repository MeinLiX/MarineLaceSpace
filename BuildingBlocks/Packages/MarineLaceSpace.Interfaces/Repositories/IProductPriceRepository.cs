using MarineLaceSpace.Models.Database.Catalog;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface IProductPriceRepository : IRepository<ProductPrice>
{
    Task<ProductPrice> GetByVariantAsync(string productId, string? sizeId = null, string? materialId = null, string? colorId = null);
    Task<decimal> GetPriceAsync(string productId, string? sizeId = null, string? materialId = null, string? colorId = null);
    Task<decimal> GetBaseProductPriceAsync(string productId);
    Task<decimal> GetMinProductPriceAsync(string productId);
    Task<decimal> GetMaxProductPriceAsync(string productId);
    Task<(decimal Min, decimal Max)> GetPriceRangeAsync(string productId);
}
