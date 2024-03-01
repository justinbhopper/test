using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Text;
using RH.Polaris.AppInit;
using RH.Polaris.HttpClient.Config.Extensions;
using TestConsole.KeyVaults;
using TestConsole.OpenAi;

namespace TestConsole;

internal static class Program
{
    public class Foo { }
    public class Bar { }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var encoder = new UTF8Encoding(false, true)
        {
            EncoderFallback = new EncoderReplacementFallback(string.Empty)
        };

        var streamPayload = new MemoryStream();
        using var streamWriter = new StreamWriter(streamPayload, encoding: encoder, bufferSize: 1024, leaveOpen: true);

        streamWriter.Write('\uD83D' + "👍🏻 👍");
        streamWriter.Flush();

        return CliAppInitHost
            .Initialize()
            .RegisterAzureServiceBusDriver()
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
        services.AddSingleton<KeyVaultTest>();
        services.AddSingleton<OpenAiTest>();

        services.AddStandardHttpClient("carefabric");
    }

    private sealed class Contributor : IAppInitContributor<IServiceCollection>
    {
        public IServiceCollection Contribute(IContributorContext context, IServiceCollection services)
        {
            services.ScanValidators<Contributor>();

            services.AddStandardHttpClient("carefabric");

            return services;
        }
    }
}
