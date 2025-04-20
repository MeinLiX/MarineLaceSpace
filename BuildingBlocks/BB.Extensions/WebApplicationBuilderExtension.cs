using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Microsoft.AspNetCore.Builder
{
    public static class WebApplicationBuilderExtension
    {
        private static readonly ConcurrentDictionary<IHostApplicationBuilder, IList<Action<WebApplication>>> _postBuildActions = new();

        public static WebApplication BuildWithPostActions(this WebApplicationBuilder builder)
        {
            var app = builder.Build();

            if (_postBuildActions.TryRemove(builder, out var actions))
            {
                foreach (var action in actions)
                {
                    try
                    {
                        action(app);
                    }
                    catch (Exception ex)
                    {
                        app.Logger.LogError(ex, "Error executing post-build action");
                    }
                }
            }

            return app;
        }

        public static IHostApplicationBuilder AddUseAfterBuild(this IHostApplicationBuilder builder, params Action<WebApplication>[] actionsToRun)
        {
            var actions = _postBuildActions.GetOrAdd(builder, _ => []);

            lock (actions)
            {
                foreach (var action in actionsToRun)
                {
                    actions.Add(action);
                }
            }

            return builder;
        }
    }
}
