namespace jdc.build.dependencyGraph {
    public class DependencyGraphBuilder {
        public async Task BuildAsync(FileInfo projectFile, IEnumerable<string> ignore, CancellationToken cancellationToken) {
            DirectoryInfo projectDir = new(Path.GetDirectoryName(projectFile.FullName) ?? "");

            //first get dependencies from project file
            //second get dependencies from nuget packages

        }
    }
}
