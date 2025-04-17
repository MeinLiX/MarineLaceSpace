using MarineLaceSpace.Models.Catalog;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface IProductPhotoRepository : IRepository<ProductPhoto>
{
    Task<IEnumerable<ProductPhoto>> GetByProductIdAsync(string productId);
    Task<IEnumerable<ProductPhoto>> GetByProductVariantAsync(string productId, string? sizeId = null, string? materialId = null, string? colorId = null);
    Task<ProductPhoto> GetMainPhotoAsync(string productId);
}
