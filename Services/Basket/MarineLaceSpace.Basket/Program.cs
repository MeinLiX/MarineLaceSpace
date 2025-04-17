var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapGet("/", (ILogger<Program> l) =>
{
    l.LogInformation("HELLO WORLD FROM LOG");
    return "basket";
});

await app.RunAsync();
