using Catalog.WebHost.Routes;
using Catalog.WebHost.Services;
using MarineLaceSpace.Catalog.Data.DBContexts;
using MarineLaceSpace.Catalog.Data.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("pg-catalog");
builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

var redisConnectionString = builder.Configuration.GetConnectionString("redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnectionString; });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddSingleton<ICategoryCacheService, CategoryCacheService>();

builder.Services.AddScoped<IShopRepository, ShopRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
builder.Services.AddScoped<IProductPriceRepository, ProductPriceRepository>();
builder.Services.AddScoped<IProductPhotoRepository, ProductPhotoRepository>();

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapShopRoutes();
app.MapProductRoutes();
app.MapCategoryRoutes();
app.MapReviewRoutes();
app.MapDictionaryRoutes();
app.MapPriceRoutes();
app.MapPhotoRoutes();

await app.RunAsync();
