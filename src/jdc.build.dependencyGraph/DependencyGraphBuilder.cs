using jdc.build.dependencyGraph.models;
using Microsoft.Extensions.Logging;

namespace jdc.build.dependencyGraph {
    public interface IDependencyGraphBuilder {
        Task BuildAsync(FileInfo projectFile, IEnumerable<string> ignore, CancellationToken cancellationToken);
    }

    public class DependencyGraphBuilder(
        ILogger<DependencyGraphBuilder> logger,
        dotnet.IProjectReader projectReader,
        nuget.ISourceReader sourceReader,
        mermaid.IGraphRenderer graphRenderer
     )
        : IDependencyGraphBuilder {
        public async Task BuildAsync(FileInfo projectFile, IEnumerable<string> ignore, CancellationToken cancellationToken) {
            DirectoryInfo projectDir = new(Path.GetDirectoryName(projectFile.FullName) ?? "");

            //first we get the need frameworks from the project file
            IEnumerable<string> frameworks = await projectReader.GetFrameworksAsync(projectFile, cancellationToken);

            foreach (string framework in frameworks) {
                var root = new DependencyNode(Path.GetFileNameWithoutExtension(projectFile.Name), "0.0.0.0");

                //first get dependencies from project file
                Dictionary<string, DependencyNode> items = new() {
                    { root.ToString(),root}
                };

                foreach (DependencyNode dep in await projectReader.GetDependenciesAsync(projectFile, framework)) {
                    string key = dep.ToString();
                    if (!root.Edges.ContainsKey(key)) {
                        root.Edges.Add(key, new DependencyEdge(dep, 1));
                    }
                    items.Add(dep.ToString(), dep);
                }

                //second get dependencies from nuget packages
                sourceReader.InitConfig(projectDir);
                foreach (DependencyNode dep in items.Values.ToArray()) {
                    await sourceReader.FetchDependenciesAsync(dep, framework, items, ignore, cancellationToken);
                }

                //last render the graph
                string mermaidGraph = graphRenderer.RenderAsMermaid(items, ignore);
                Console.WriteLine(mermaidGraph);

            }



        }
    }
}
