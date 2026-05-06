using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BB.Common.Extensions;

public static class DataExtensions
{
    public static async Task MigrateWithRetryAsync<TContext>(this IServiceProvider serviceProvider, int maxRetries = 5, int baseDelayMs = 1000) where TContext : DbContext
    {
        var logger = serviceProvider.GetRequiredService<ILogger<TContext>>();
        var db = serviceProvider.GetRequiredService<TContext>();

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                logger.LogInformation("Database migration completed successfully for {Context}", typeof(TContext).Name);
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                var delay = baseDelayMs * attempt;
                logger.LogWarning(ex, "Database migration attempt {Attempt}/{MaxRetries} failed for {Context}. Retrying in {Delay}ms...",
                    attempt, maxRetries, typeof(TContext).Name, delay);
                await Task.Delay(delay);
            }
        }
    }

    public static async Task EnsureCreatedWithRetryAsync<TContext>(this IServiceProvider serviceProvider, int maxRetries = 5, int baseDelayMs = 1000) where TContext : DbContext
    {
        var logger = serviceProvider.GetRequiredService<ILogger<TContext>>();
        var db = serviceProvider.GetRequiredService<TContext>();

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await db.Database.EnsureCreatedAsync();
                logger.LogInformation("Database ensured created for {Context}", typeof(TContext).Name);
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                var delay = baseDelayMs * attempt;
                logger.LogWarning(ex, "EnsureCreated attempt {Attempt}/{MaxRetries} failed for {Context}. Retrying in {Delay}ms...",
                    attempt, maxRetries, typeof(TContext).Name, delay);
                await Task.Delay(delay);
            }
        }
    }
}
