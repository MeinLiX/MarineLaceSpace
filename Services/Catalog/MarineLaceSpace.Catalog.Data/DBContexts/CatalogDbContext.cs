using BB.Common.Data.Converters;
using MarineLaceSpace.Enumerations;
using MarineLaceSpace.Models.Database.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MarineLaceSpace.Catalog.Data.DBContexts;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<Size> Sizes { get; set; }

    public DbSet<Shop> Shops { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }

    public DbSet<ProductColor> ProductColors { get; set; }
    public DbSet<ProductMaterial> ProductMaterials { get; set; }
    public DbSet<ProductSize> ProductSizes { get; set; }
    public DbSet<ProductPhoto> ProductPhotos { get; set; }
    public DbSet<ProductPrice> ProductPrices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasIndex(s => s.UrlSlug).IsUnique();
            entity.HasMany(s => s.Products)
                  .WithOne(p => p.Shop)
                  .HasForeignKey(p => p.ShopId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasOne(c => c.ParentCategory)
                  .WithMany(c => c.Subcategories)
                  .HasForeignKey(c => c.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.CategoryId);
            entity.HasIndex(p => p.ShopId);
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.Property(e => e.Gender)
                  .HasConversion(new EnumerationConverter<ProductSizeGender>());
        });
    }
}