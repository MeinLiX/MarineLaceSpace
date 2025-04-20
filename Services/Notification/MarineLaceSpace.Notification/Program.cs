var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.BuildWithPostActions();

await app.RunAsync();
