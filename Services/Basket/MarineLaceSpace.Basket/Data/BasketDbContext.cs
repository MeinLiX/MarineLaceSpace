using MarineLaceSpace.Models.Database.Basket;
using Microsoft.EntityFrameworkCore;

namespace Basket.WebHost.Data;

public class BasketDbContext(DbContextOptions<BasketDbContext> options) : DbContext(options)
{
    public DbSet<BasketEntity> Baskets { get; set; }
}
