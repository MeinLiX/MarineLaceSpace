using Catalog.WebHost.Routes;
using MarineLaceSpace.Catalog.Data.DBContexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("pg-catalog");

builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

app.MapShopRoutes();

await app.RunAsync();
