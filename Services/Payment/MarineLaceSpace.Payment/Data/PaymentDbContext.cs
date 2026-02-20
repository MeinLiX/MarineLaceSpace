using MarineLaceSpace.Models.Database.Payment;
using Microsoft.EntityFrameworkCore;

namespace Payment.WebHost.Data;

public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<PaymentRecord> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PaymentRecord>(entity =>
        {
            entity.HasIndex(p => p.OrderId);
            entity.Property(p => p.Amount).HasPrecision(18, 2);
        });
    }
}
