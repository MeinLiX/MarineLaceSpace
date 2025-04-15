using MarineLaceSpace.Models.Catalog;

namespace MarineLaceSpace.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetByParentIdAsync(int? parentId);
    Task<string> GetFullPathAsync(int categoryId);
    Task<IEnumerable<Category>> GetCategoryPathAsync(int categoryId);
    Task UpdateChildCategoriesPathAsync(Category parentCategory);
}
