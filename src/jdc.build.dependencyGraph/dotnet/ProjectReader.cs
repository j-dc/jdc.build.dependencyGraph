using jdc.build.dependencyGraph.models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
namespace jdc.build.dependencyGraph.dotnet;

public interface IProjectReader {
    Task<IEnumerable<string>> GetFrameworksAsync(FileInfo projectPath, CancellationToken token);

    Task<IEnumerable<DependencyNode>> GetDependenciesAsync(FileInfo projectPath, string targetFramework);

}

public partial class ProjectReader(
    ILogger<ProjectReader> logger
) : IProjectReader {
    public async Task<IEnumerable<string>> GetFrameworksAsync(FileInfo projectPath, CancellationToken token) {
        if (!projectPath.Exists) {
            throw new FileNotFoundException("Project file not found.", projectPath.FullName);
        }

        using var ms = new FileStream(projectPath.FullName, FileMode.Open, FileAccess.Read);
        XDocument doc = await XDocument.LoadAsync(ms, LoadOptions.None, token);
        XNamespace ns = doc.Root!.Name.Namespace;
        XElement? propertyGroup = doc.Descendants(ns + "PropertyGroup").FirstOrDefault();

        if (propertyGroup == null) {
            return [];
        }

        string? tfms = propertyGroup.Element(ns + "TargetFrameworks")?.Value;
        if (!string.IsNullOrEmpty(tfms)) {
            return tfms.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }

        string? tfm = propertyGroup.Element(ns + "TargetFramework")?.Value;
        if (!string.IsNullOrEmpty(tfm)) {
            return [tfm];
        }
        return [];
    }
    public async Task<IEnumerable<DependencyNode>> GetDependenciesAsync(FileInfo projectPath, string targetFramework) {
        var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "dotnet",
                Arguments = $"list \"{projectPath}\" package --framework {targetFramework}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var outputBuilder = new StringBuilder();
        process.OutputDataReceived += (sender, args) => {
            if (args.Data != null) {
                outputBuilder.AppendLine(args.Data);
            }
        };
        process.Start();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync();
        string output = outputBuilder.ToString();
        var packages = new List<DependencyNode>();
        // Locate the section for the specified framework     

        string frameworkHeader = $" [{targetFramework}]:";
        int frameworkIndex = output.IndexOf(frameworkHeader, StringComparison.OrdinalIgnoreCase);
        if (frameworkIndex == -1) {
            return packages;
        }

        string frameworkSection = output[frameworkIndex..];

        // Regex to match top-level package lines     
        Regex regex = FindReferencesRegex();
        foreach (GroupCollection? groups in regex
            .Matches(frameworkSection)
            .Select(match => match.Groups)
        ) {
            string packageId = groups[1].Value;
            string resolved = groups[2].Value;
            packages.Add(new(packageId, resolved));
        }
        return packages;
    }

    [GeneratedRegex(@">\s+(\S+)\s+\S+\s+(\S+)", RegexOptions.Compiled)]
    private static partial Regex FindReferencesRegex();
}
