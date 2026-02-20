using MarineLaceSpace.Catalog.Data.DBContexts;
using MarineLaceSpace.Exceptions.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using MarineLaceSpace.Models.Database.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MarineLaceSpace.Catalog.Data.Repositories;

public class CategoryRepository(CatalogDbContext context) : ICategoryRepository
{
    private readonly CatalogDbContext _context = context;

    public async Task<Category> GetByIdAsync(string entityId)
    {
        return await _context.Categories
                   .Include(c => c.Subcategories)
                   .FirstOrDefaultAsync(c => c.Id == entityId)
               ?? throw new NotFoundEntityException($"Category with ID '{entityId}' not found.");
    }

    public async Task<IEnumerable<Category>> GetByParentIdAsync(string? parentId)
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == parentId)
            .Include(c => c.Subcategories)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<string> GetFullPathAsync(string categoryId)
    {
        var category = await GetByIdAsync(categoryId);
        return category.FullPath ?? category.Name;
    }

    public async Task<IEnumerable<Category>> GetCategoryPathAsync(string categoryId)
    {
        var path = new List<Category>();
        var current = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == categoryId);

        while (current != null)
        {
            path.Insert(0, current);
            if (current.ParentCategoryId != null)
                current = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == current.ParentCategoryId);
            else
                current = null;
        }

        return path;
    }

    public async Task UpdateChildCategoriesPathAsync(Category parentCategory)
    {
        var children = await _context.Categories
            .Where(c => c.ParentCategoryId == parentCategory.Id)
            .ToListAsync();

        foreach (var child in children)
        {
            child.FullPath = $"{parentCategory.FullPath} > {child.Name}";
            child.Level = parentCategory.Level + 1;
            await UpdateChildCategoriesPathAsync(child);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Category> AddAsync(Category entity)
    {
        if (entity.ParentCategoryId != null)
        {
            var parent = await GetByIdAsync(entity.ParentCategoryId);
            entity.Level = parent.Level + 1;
            entity.FullPath = $"{parent.FullPath} > {entity.Name}";
        }
        else
        {
            entity.Level = 0;
            entity.FullPath = entity.Name;
        }

        await _context.Categories.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Category entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        await UpdateChildCategoriesPathAsync(entity);
    }

    public async Task DeleteAsync(string entityId)
    {
        var category = await GetByIdAsync(entityId);
        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == entityId);
        if (hasProducts)
            throw new ValidationEntityException("Cannot delete category that has products.");
        
        var hasChildren = await _context.Categories.AnyAsync(c => c.ParentCategoryId == entityId);
        if (hasChildren)
            throw new ValidationEntityException("Cannot delete category that has subcategories.");
        
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}
