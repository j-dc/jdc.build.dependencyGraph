namespace jdc.build.dependencyGraph {
    public interface IDependencyGraphBuilder {
        Task BuildAsync(FileInfo projectFile, IEnumerable<string> ignore, CancellationToken cancellationToken);
    }

    public class DependencyGraphBuilder : IDependencyGraphBuilder {
        public async Task BuildAsync(FileInfo projectFile, IEnumerable<string> ignore, CancellationToken cancellationToken) {
            DirectoryInfo projectDir = new(Path.GetDirectoryName(projectFile.FullName) ?? "");

            //first get dependencies from project file
            //second get dependencies from nuget packages

        }
    }
}
