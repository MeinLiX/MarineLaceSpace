using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using BB.Extensions;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Hosting;

// https://aka.ms/dotnet/aspire/service-defaults
public static class TBuilderExtensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder, bool useCommonAuthentication = true, bool useCommonOpenapi = true, bool useCommonOptions = true) where TBuilder : IHostApplicationBuilder
    {
        if (useCommonAuthentication)
        {
            builder.Services.AddCommonAuthenticationApi();
        }
        if (useCommonOpenapi)
        {
            builder.Services.AddCommonOpenApi();
            builder.AddUseAfterBuild(app => app.UseCommonScalar());
        }

        if (useCommonOptions)
        {
            builder.Services.AddCommonOptions(builder.Configuration);
        }

        builder.AddUseAfterBuild(app => app.MapDefaultEndpoints());

        return builder.AddServiceDefaults();
    }

    /// <summary>
    /// Required <see cref="ServiceDiscoveryServiceCollectionExtensions.AddServiceDiscovery"/> 
    /// and <see cref="HttpClientFactoryServiceCollectionExtensions.ConfigureHttpClientDefaults"/>
    /// </summary>
    private static TBuilder AddLocalHttpClientsByProjectName<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        if (builder.Configuration.GetSection("Services") is IConfigurationSection servicesSection)
        {
            foreach (var serviceConfig in servicesSection.GetChildren())
            {
                var serviceName = serviceConfig.Key;
                builder.Services.AddHttpClient(serviceName, client =>
                {
                    client.BaseAddress = new Uri($"http://{serviceName}");
                });
            }
        }
        return builder;
    }

    private static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();

            http.AddServiceDiscovery();
            
        });
        builder.AddLocalHttpClientsByProjectName();

        builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        {
            options.AllowedSchemes = ["https"];
        });

        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            if (builder.Environment.IsDevelopment())
            {
                logging.AddDebug();
            }
            logging.AddEventSourceLogger();
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/health");

            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }

}
