using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using RH.Apollo.Persistence.Search;
using RH.Polaris.AppInit;
using RH.Polaris.HttpClient.Config.Extensions;
using TestConsole.KeyVaults;
using TestConsole.Netsmart;
using TestConsole.OpenAi;

namespace TestConsole;

internal static class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return CliAppInitHost
            .Initialize()
            .RegisterTheiaClients()
            .RegisterAzureServiceBusDriver()
            .AddOptions<CareFabricConfig>()
            .AddOptions<OpenAiConfig>()
            .Add<Contributor>()
            .CreateBuilder(args, ConfigureServices);
    }

    public static async Task<int> Main(string[] args)
    {
        return await CreateHostBuilder(args).RunHost();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<TestCommand>();
        services.AddSingleton<NetsmartTest>();
        services.AddSingleton<KeyVaultTest>();
        services.AddSingleton<OpenAiTest>();

        services
            .AddSingleton<SearchIndexResolver>()
            .AddTransient<SearchIndexer>()
            .AddTransient<FileValueExtractorFactory>();

        services.Configure<CareFabricConfig>(cfg =>
        {
            cfg.EnableEncryption = false;
            cfg.InstanceId = "BellsNotes";
        });

        services.AddStandardHttpClient("carefabric");
    }

    private sealed class Contributor : IAppInitContributor<IServiceCollection>
    {
        public IServiceCollection Contribute(IContributorContext context, IServiceCollection services)
        {
            services.ScanValidators<Contributor>();
            services
                .AddTransient<ICareFabricClientFactory, CareFabricClientFactory>()
                .AddTransient<IConfigureOptions<HttpClientFactoryOptions>, CareFabricClientConfigurator>();

            services.AddStandardHttpClient("carefabric");

            return services;
        }
    }
}
