using Auth.WebHost.Routes;
using BB.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

app.InitRoutes();

await app.RunAsync();
