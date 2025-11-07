using Microsoft.Extensions.Hosting;

static class Program {
    static async Task<int> Main(string[] args) {
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => {
                services.AddHostedService<DependencyGraphService>();
            })
            .Build();

        await host.RunAsync();
    }
}