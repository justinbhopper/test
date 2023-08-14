using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Polly;
using RH.Polaris.HttpClient.Config;

namespace TestConsole.Netsmart;

public class CareFabricClientConfigurator : IConfigureNamedOptions<HttpClientFactoryOptions>
{
    private readonly IHttpClientConfigurationProvider _provider;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public CareFabricClientConfigurator(IHttpClientConfigurationProvider provider)
    {
        _provider = provider;

        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(response =>
            {
                return response.IsTransientErrorStatus();
            })
            .WaitAndRetryAsync(2, retryCount => TimeSpan.FromSeconds(retryCount), LogRetryEvent);
    }

    public void Configure(HttpClientFactoryOptions options)
    {
        Configure(Options.DefaultName, options);
    }

    public void Configure(string? name, HttpClientFactoryOptions options)
    {
        if (name == "carefabric")
        {
            var config = _provider.Get(name);
            options.HttpMessageHandlerBuilderActions.Add(builder =>
            {
                builder.AdditionalHandlers.Add(new CareFabricErrorDetectorMessageHandler(config));
                builder.AdditionalHandlers.Add(new PolicyHttpMessageHandler(_retryPolicy));
            });
        }
    }

    private void LogRetryEvent(DelegateResult<HttpResponseMessage> outcome, TimeSpan sleepDuration)
    {
        Console.WriteLine("Retrying care fabric call...");
    }
}
