using Auth.WebHost.Routes;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

app.InitRoutes();

await app.RunAsync();
