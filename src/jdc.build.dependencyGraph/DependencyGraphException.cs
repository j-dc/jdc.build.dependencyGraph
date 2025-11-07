namespace jdc.build.dependencyGraph {
    public class DependencyGraphException : Exception {
        public DependencyGraphException() {
        }

        public DependencyGraphException(string message) : base(message) {
        }

        public DependencyGraphException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
