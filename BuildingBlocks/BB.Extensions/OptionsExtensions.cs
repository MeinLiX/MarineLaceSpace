using MarineLaceSpace.Options.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BB.Extensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddCommonOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var keys = configuration.GetSection("CommonConfiguration").Get<string[]>();

        if (keys == null || keys.Length == 0)
            return services;

        return services.AddLocalOptions(configuration, keys);
    }

    public static IServiceCollection AddLocalOptions(this IServiceCollection services, IConfiguration configuration, params string[] optionNames)
    {
        var optionsAssembly = typeof(OptionAttribute).Assembly;
        var optionTypes = optionsAssembly.GetTypes()
            .Where(t => t.GetCustomAttribute<OptionAttribute>() != null)
            .ToList();

        foreach (var optionName in optionNames)
        {
            var optionType = optionTypes.FirstOrDefault(t =>
            {
                var attr = t.GetCustomAttribute<OptionAttribute>();
                return attr != null && attr.SectionName == optionName;
            });

            if (optionType != null)
            {
                var method = (typeof(OptionsConfigurationServiceCollectionExtensions)
                    .GetMethod("Configure", [typeof(IServiceCollection), typeof(IConfiguration)])
                    ?.MakeGenericMethod(optionType)) ?? throw new InvalidOperationException($"Could not find Configure method for type {optionType.Name}");
                method.Invoke(null, [services, configuration.GetSection(optionName)]);

                services.AddSingleton(optionType, sp =>
                {
                    var options = sp.GetService(typeof(Microsoft.Extensions.Options.IOptions<>).MakeGenericType(optionType)) ?? throw new InvalidOperationException($"Could not resolve options for type {optionType.Name}");
                    var valueProperty = options.GetType().GetProperty("Value");
                    return valueProperty?.GetValue(options) ?? throw new InvalidOperationException("");
                });
            }
        }

        return services;
    }
}
