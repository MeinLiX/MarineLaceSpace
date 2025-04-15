using MarineLaceSpace.Models.Catalog;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface IProductReviewRepository : IRepository<ProductReview>
{
    Task<IEnumerable<ProductReview>> GetByProductIdAsync(int productId);
    Task<decimal> GetAverageRatingAsync(int productId);
}
