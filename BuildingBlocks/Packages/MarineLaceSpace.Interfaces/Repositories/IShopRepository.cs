using MarineLaceSpace.Models.Database.Catalog;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface IShopRepository : IRepository<Shop, string>
{
    Task<Shop> GetBySlugAsync(string slug);

    Task<Shop> GetByOwnerIdAsync(string ownerId);
}