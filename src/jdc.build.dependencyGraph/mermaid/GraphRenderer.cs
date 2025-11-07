using jdc.build.dependencyGraph.models;

namespace jdc.build.dependencyGraph.mermaid {

    public interface IGraphRenderer {
        string RenderAsMermaid(Dictionary<string, DependencyNode> items, IEnumerable<string> ignore);
    }

    public class GraphRenderer : IGraphRenderer {

        public string RenderAsMermaid(Dictionary<string, DependencyNode> items, IEnumerable<string> ignore) {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("graph LR");

            foreach (DependencyNode x in items.Values) {
                foreach (DependencyNode? y in x.Edges.Values.Select(z => z.ConnectedTo)) {
                    if (!IsIgnored(x.PackageId, ignore)) {
                        sb.AppendLine($"\t{x.PackageId} --> {y.PackageId}");
                    }
                }
            }
            return sb.ToString();
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
}
