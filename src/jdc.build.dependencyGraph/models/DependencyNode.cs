namespace jdc.build.dependencyGraph.models {
    public record DependencyNode(string PackageId, string Version) {
        public override string ToString() => $"{PackageId},{Version}";

        public Dictionary<string, DependencyEdge> Edges { get; } = [];
    }
}
