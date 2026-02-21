using Catalog.WebHost.Routes;
using Catalog.WebHost.Services;
using MarineLaceSpace.Catalog.Data.DBContexts;
using MarineLaceSpace.Catalog.Data.Repositories;
using MarineLaceSpace.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Minio;

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

var minioConnectionString = builder.Configuration.GetConnectionString("minio");
if (!string.IsNullOrEmpty(minioConnectionString))
{
    var uri = new Uri(minioConnectionString);
    var minioEndpoint = uri.Authority;
    var minioAccessKey = builder.Configuration["Minio:AccessKey"] ?? "minioadmin";
    var minioSecretKey = builder.Configuration["Minio:SecretKey"] ?? "minioadmin";

    builder.Services.AddMinio(configureClient => configureClient
        .WithEndpoint(minioEndpoint)
        .WithCredentials(minioAccessKey, minioSecretKey)
        .WithSSL(uri.Scheme == "https")
        .Build());
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
    await db.Database.MigrateAsync();
}

app.MapShopRoutes();
app.MapProductRoutes();
app.MapCategoryRoutes();
app.MapReviewRoutes();
app.MapDictionaryRoutes();
app.MapPriceRoutes();
app.MapPhotoRoutes();

await app.RunAsync();
