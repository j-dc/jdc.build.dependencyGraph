using jdc.build.dependencyGraph.models;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
namespace jdc.build.dependencyGraph.nuget;

public interface ISourceReader {
    void InitConfig(DirectoryInfo startDirectory);
    Task FetchDependenciesAsync(DependencyNode parent, string targetFramework, Dictionary<string, DependencyNode> items, IEnumerable<string> ignore, CancellationToken token);
}

public class SourceReader(ILogger<SourceReader> logger) : ISourceReader {
    private IEnumerable<PackageSource>? _packageSources;
    private PackageSourceMapping? _packageSourceMapping;
    public void InitConfig(DirectoryInfo startDirectory) {
        DirectoryInfo configDir = FindConfigFile(startDirectory) ?? throw new DependencyGraphException("config file not found");
        //READING NUGET     
        ISettings settings = Settings.LoadSpecificSettings(configDir.FullName, "Nuget.config");
        //READING SOURCES     
        var packageSourceProvider = new PackageSourceProvider(settings);
        _packageSources = packageSourceProvider
            .LoadPackageSources()
            .Where(s => s.IsEnabled)
            ;

        //READING MAPPING
        _packageSourceMapping = PackageSourceMapping.GetPackageSourceMapping(settings);
    }

    private static DirectoryInfo? FindConfigFile(DirectoryInfo startDirectory) {
        DirectoryInfo? currentDir = startDirectory;
        while (currentDir != null) {
            string configPath = Path.Combine(currentDir.FullName, "Nuget.config");
            if (File.Exists(configPath)) {
                return currentDir;
            }
            currentDir = currentDir.Parent;
        }
        return null;
        // Not found
    }

    public async Task FetchDependenciesAsync(DependencyNode parent, string targetFramework, Dictionary<string, DependencyNode> items, IEnumerable<string> ignore, CancellationToken token) {
        if (_packageSourceMapping is null || _packageSources is null) {
            throw new DependencyGraphException("InitConfig should be called first");
        }
        // Find the correct source for the package    
        string? matchingSourceName = _packageSourceMapping
             .GetConfiguredPackageSources(parent.PackageId)[0];

        if (string.IsNullOrWhiteSpace(matchingSourceName)) {
            logger.LogInformation("No source mapping found for package {PackageId}", parent.PackageId);
            return;
        }

        PackageSource? matchingSource = _packageSources.FirstOrDefault(s => s.Name.Equals(matchingSourceName, StringComparison.OrdinalIgnoreCase));
        if (matchingSource == null) {
            logger.LogInformation("Mapped source '{SourceName}' not found among enabled sources.", matchingSourceName);
            return;
        }
        logger.LogInformation("Using mapped source: {Source}", matchingSource.Source);
        IEnumerable<Lazy<INuGetResourceProvider>> providers = Repository.Provider.GetCoreV3();
        var repository = new SourceRepository(matchingSource, providers);
        PackageMetadataResource metadataResource = await repository.GetResourceAsync<PackageMetadataResource>(token);
        IEnumerable<IPackageSearchMetadata> packageMetadata = await metadataResource.GetMetadataAsync(parent.PackageId, includePrerelease: false, includeUnlisted: false, new SourceCacheContext(), NullLogger.Instance, token);
        IPackageSearchMetadata? package = packageMetadata?.FirstOrDefault(p => p.Identity.Version == NuGetVersion.Parse(parent.Version));
        if (package == null) {
            logger.LogInformation("Package not found.");
            return;
        }

        IEnumerable<PackageDependencyGroup> dependencyGroups = package.DependencySets;
        foreach (PackageDependencyGroup? group in dependencyGroups) {
            foreach (PackageDependency? dependency in group.Packages) {
                logger.LogInformation(" - {DependencyId} {VersionsRange}", dependency.Id, dependency.VersionRange);
                var newChild = new DependencyNode(dependency.Id, dependency.VersionRange.MinVersion.ToString());
                string newChildKey = newChild.ToString();
                if (!items.TryGetValue(newChildKey, out DependencyNode? existingNewItem)) {
                    if (!IsIgnored(newChild.PackageId, ignore)) {
                        items.Add(newChildKey, newChild);
                        await FetchDependenciesAsync(newChild, targetFramework, items, ignore, token);
                    }
                } else {
                    newChild = existingNewItem;
                }
                parent.Edges.TryAdd(newChildKey, new DependencyEdge(newChild, 1));
            }
        }
    }

    private static bool IsIgnored(string packageid, IEnumerable<string> ignore) {
        foreach (string dep in ignore) {
            if (packageid.StartsWith(dep, StringComparison.CurrentCultureIgnoreCase)) {
                return true;
            }
        }
        return false;
    }
}