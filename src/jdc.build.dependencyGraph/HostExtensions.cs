using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace jdc.build.dependencyGraph {
    public static class HostExtensions {
        public static IHostBuilder UseDependencyGraph(this IHostBuilder hostBuilder, string[] args) =>
            hostBuilder.ConfigureServices((hostContext, services) => {
                services.AddHostedService<DependencyGraphService>();
                services.AddScoped<IDependencyGraphBuilder, DependencyGraphBuilder>();
            })
            .ConfigureAppConfiguration((hostingContext, config) => {
                config.AddCommandLine(args);
            })

            ;

    }
}
