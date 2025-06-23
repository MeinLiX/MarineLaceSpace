using Catalog.WebHost.Routes;
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

builder.Services.AddScoped<IShopRepository, ShopRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

app.MapShopRoutes();
app.MapProductRoutes();

await app.RunAsync();
