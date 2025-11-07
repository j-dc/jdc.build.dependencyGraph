using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace jdc.build.dependencyGraph {
    public static class HostExtensions {
        public static IHostBuilder UseDependencyGraph(this IHostBuilder hostBuilder, string[] args) =>
            hostBuilder.ConfigureServices((hostContext, services) => {
                services.AddHostedService<DependencyGraphService>();
                services.AddScoped<IDependencyGraphBuilder, DependencyGraphBuilder>();
                services.AddScoped<dotnet.IProjectReader, dotnet.ProjectReader>();
                services.AddScoped<nuget.ISourceReader, nuget.SourceReader>();
                services.AddScoped<mermaid.IGraphRenderer, mermaid.GraphRenderer>();
            })
            .ConfigureLogging((ctx, logging) => {
                logging.ClearProviders();
                if (!string.IsNullOrWhiteSpace(ctx.Configuration["verbose"])) {

                    logging.AddConsole();
                }
            })
            .ConfigureAppConfiguration((ctx, config) => {
                config.AddCommandLine(args);
            })

            ;

    }
}
