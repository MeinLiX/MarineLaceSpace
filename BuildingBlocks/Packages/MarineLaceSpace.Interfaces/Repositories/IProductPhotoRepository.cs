using MarineLaceSpace.Models.Catalog;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface IProductPhotoRepository : IRepository<ProductPhoto>
{
    Task<IEnumerable<ProductPhoto>> GetByProductIdAsync(int productId);
    Task<IEnumerable<ProductPhoto>> GetByProductVariantAsync(int productId, int? sizeId = null, int? materialId = null, int? colorId = null);
    Task<ProductPhoto> GetMainPhotoAsync(int productId);
}
