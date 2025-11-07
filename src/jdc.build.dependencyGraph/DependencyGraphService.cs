using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace jdc.build.dependencyGraph {
    public class DependencyGraphService(
        ILogger<DependencyGraphService> logger,
        IConfiguration configuration,
        IDependencyGraphBuilder graphBuilder,
        IHostApplicationLifetime hostApplicationLifetime
    ) : BackgroundService {

        private const string _parProjectFile = "projectFile";
        private const string _parIgnore = "ignore";
        private const string _parIncludeFW = "includeFramework";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                var fi = new FileInfo(configuration[_parProjectFile] ?? "");
                if (!fi.Exists) {
                    throw new FileNotFoundException("A project file is needed", fi.FullName);
                }
                List<string> ignore = [];
                ignore.AddRange((configuration[_parIgnore] ?? "").Split([';', ','], StringSplitOptions.RemoveEmptyEntries));

                if (string.IsNullOrEmpty(configuration[_parIncludeFW])) {
                    ignore.AddRange(["Microsoft.", "System.", "runtime", "NETStandard."]);
                }

                await graphBuilder.BuildAsync(fi, ignore, stoppingToken);
            } catch (Exception ex) {
                logger.LogError(ex, "An error occurred while building the dependency graph.");
            }
            hostApplicationLifetime.StopApplication();
        }
    }
}
