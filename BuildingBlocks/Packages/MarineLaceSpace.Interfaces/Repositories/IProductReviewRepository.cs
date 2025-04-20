using MarineLaceSpace.Models.Database.Catalog;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface IProductReviewRepository : IRepository<ProductReview>
{
    Task<IEnumerable<ProductReview>> GetByProductIdAsync(string productId);
    Task<decimal> GetAverageRatingAsync(string productId);
}
