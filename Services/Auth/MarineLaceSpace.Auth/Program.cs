using Auth.WebHost.Data;
using Auth.WebHost.Routes;
using Auth.WebHost.Services;
using BB.Common.Data.Repositories;
using BB.Common.EventBus;
using MarineLaceSpace.Interfaces.EventBus;
using MarineLaceSpace.Interfaces.Repositories.Auth;
using MarineLaceSpace.Models.Database.Auth;
using MarineLaceSpace.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("pg-identity");
builder.Services.AddDbContext<AuthIdentityDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddIdentityCore<AuthUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddSignInManager()
.AddEntityFrameworkStores<AuthIdentityDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});

builder.Services.AddScoped<BB.Common.Data.DBContexts.AuthDbContext>(sp =>
    sp.GetRequiredService<AuthIdentityDbContext>());
builder.Services.AddScoped<IAuthUserRepository, AuthUserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddHostedService<TokenCleanupService>();

var rabbitConnectionString = builder.Configuration.GetConnectionString("rabbitmq");
if (!string.IsNullOrEmpty(rabbitConnectionString))
{
    builder.Services.AddRabbitMQEventBus(rabbitConnectionString, "auth-api");
}

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthIdentityDbContext>();
    await db.Database.EnsureCreatedAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = ["Admin", "Seller", "Customer", "Anonimous"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Seed default admin user
    var adminSeed = app.Configuration.GetSection("AdminSeed").Get<AdminSeedOption>();
    if (adminSeed is { Email.Length: > 0, Password.Length: > 0 })
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthUser>>();
        var existingAdmin = await userManager.FindByEmailAsync(adminSeed.Email);
        if (existingAdmin is null)
        {
            var adminUser = new AuthUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = adminSeed.Email,
                Email = adminSeed.Email,
                EmailConfirmed = true,
                FirstName = adminSeed.FirstName,
                LastName = adminSeed.LastName,
            };

            var result = await userManager.CreateAsync(adminUser, adminSeed.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                app.Logger.LogInformation("Default admin user seeded: {Email}", adminSeed.Email);
            }
            else
            {
                app.Logger.LogWarning("Failed to seed admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}

app.InitRoutes();

await app.RunAsync();
