using MarineLaceSpace.Models.Catalog;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetByParentIdAsync(string? parentId);
    Task<string> GetFullPathAsync(string categoryId);
    Task<IEnumerable<Category>> GetCategoryPathAsync(string categoryId);
    Task UpdateChildCategoriesPathAsync(Category parentCategory);
}
