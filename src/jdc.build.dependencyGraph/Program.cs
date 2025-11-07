using jdc.build.dependencyGraph;
using Microsoft.Extensions.Hosting;

static class Program {
    static async Task<int> Main(string[] args) {
        IHost host = Host.CreateDefaultBuilder(args)
            .UseDependencyGraph(args)
            .Build();

        await host.RunAsync();
        return 0;
    }
}