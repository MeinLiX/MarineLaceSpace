using Microsoft.EntityFrameworkCore;

namespace Order.WebHost.Data;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<MarineLaceSpace.Models.Database.Order.Order> Orders { get; set; }
    public DbSet<MarineLaceSpace.Models.Database.Order.OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MarineLaceSpace.Models.Database.Order.Order>(entity =>
        {
            entity.HasIndex(o => o.BuyerId);
            entity.HasIndex(o => o.ShopId);
            entity.HasIndex(o => o.StatusId);
            entity.HasIndex(o => o.CreatedAt);
            entity.Property(o => o.TotalPrice).HasPrecision(18, 2);
            entity.HasMany(o => o.Items)
                  .WithOne(i => i.Order)
                  .HasForeignKey(i => i.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MarineLaceSpace.Models.Database.Order.OrderItem>(entity =>
        {
            entity.HasIndex(i => i.OrderId);
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
        });
    }
}
