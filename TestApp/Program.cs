using Microsoft.Extensions.Hosting;
using RH.Polaris.AppInit;
using RH.Polaris.Bus;

namespace TestApp;

internal static class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return ContainerAppInitHost
            .Initialize()
            .RegisterAzureServiceBusDriver()
            .RegisterZoneBus()
            .CreateBuilder<Startup>(args);
    }

    public static async Task<int> Main(string[] args)
    {
        return await CreateHostBuilder(args).RunHost();
    }
}
