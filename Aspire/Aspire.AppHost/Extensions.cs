using Microsoft.Extensions.Configuration;

namespace Aspire.AppHost;

public static class Extensions
{
    #region Configuration extensions

    public static IResourceBuilder<T> AddCommonConfiguration<T>(this IResourceBuilder<T> builder, IConfigurationManager configuration) where T : IResourceWithEnvironment
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);

        var keys = configuration.GetSection("CommonConfiguration").Get<string[]>();

        if (keys == null || keys.Length == 0)
            return builder;

        return builder.AddSectionConfiguration(configuration, keys);
    }

    public static IResourceBuilder<T> AddSectionConfiguration<T>(this IResourceBuilder<T> builder, IConfigurationManager configuration, params IEnumerable<string> keys) where T : IResourceWithEnvironment
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);
        
        foreach (var configSection in configuration.GetChildren())
        {
            if (!keys.Contains(configSection.Key))
                continue;

            if (configSection.Value != null)
            {
                builder.WithEnvironment(configSection.Key, configSection.Value);
                continue;
            }

            ProcessConfigurationSection(builder, configSection, configSection.Key);
        }

        return builder;
    }

    private static void ProcessConfigurationSection<T>(IResourceBuilder<T> builder, IConfigurationSection section, string prefix) where T : IResourceWithEnvironment
    {
        foreach (var child in section.GetChildren())
            if (child.Value != null) builder.WithEnvironment($"{prefix}__{child.Key}", child.Value);
            else ProcessConfigurationSection(builder, child, $"{prefix}__{child.Key}");
    }
    #endregion Configuration extensions
}
