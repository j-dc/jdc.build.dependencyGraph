using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace jdc.build.dependencyGraph {
    public class DependencyGraphService(
        ILogger<DependencyGraphService> logger,
        IConfiguration configuration,
        Id
    ) : BackgroundService {

        private const string _parProjectFile = "projectFile";
        private const string _parIgnore = "ignore";
        protected override Task ExecuteAsync(CancellationToken stoppingToken) {
            var fi = new FileInfo(configuration[_parProjectFile] ?? "");
            if (!fi.Exists) {
                throw new FileNotFoundException("A project file is needed", fi.FullName);
            }
            string[] ignore = (configuration[_parIgnore] ?? "").Split([';', ','], StringSplitOptions.RemoveEmptyEntries);

        }

    }
}
