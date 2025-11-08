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
                FileInfo? fi = null;
                string pf = configuration[_parProjectFile] ?? "";
                if (string.IsNullOrWhiteSpace(pf)
                    && (
                        Boolean.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ?? "false", out bool incont)
                        && incont
                    )
                ) {
                    var di = new DirectoryInfo("/src");
                    if (di.Exists) {
                        var projFile = di.GetFiles("*.*proj").FirstOrDefault();
                        if (projFile is not null) {
                            fi = projFile;
                        }
                    }
                } else {
                    fi = new FileInfo(pf);

                }

                if (fi is null) {
                    throw new DependencyGraphException("A project file could not be defined.");
                }
                if (!fi.Exists) {
                    throw new FileNotFoundException("The given project file could not be found.", fi.FullName);
                }

                logger.LogInformation("Building dependency graph for project file: {ProjectFile}", fi.FullName);

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
