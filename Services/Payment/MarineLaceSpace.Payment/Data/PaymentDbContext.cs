using MarineLaceSpace.Models.Database.Payment;
using Microsoft.EntityFrameworkCore;

namespace Payment.WebHost.Data;

public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<PaymentRecord> Payments { get; set; }
    public DbSet<RefundRecord> Refunds { get; set; }
    public DbSet<PaymentStatusHistory> PaymentStatusHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PaymentRecord>(entity =>
        {
            entity.HasIndex(p => p.OrderId);
            entity.HasIndex(p => p.StatusId);
            entity.Property(p => p.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<RefundRecord>(entity =>
        {
            entity.HasIndex(r => r.PaymentId);
            entity.Property(r => r.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<PaymentStatusHistory>(entity =>
        {
            entity.HasIndex(h => h.PaymentId);
            entity.HasIndex(h => h.ChangedAt);
        });
    }
}
