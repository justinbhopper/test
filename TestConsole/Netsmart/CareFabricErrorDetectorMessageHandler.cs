using System.Net;
using Netsmart.Bedrock.CareFabric.Core.Enumerations;
using Netsmart.Bedrock.CareFabric.Core.Rigging;
using RH.Polaris.HttpClient.Config;
using RH.Polaris.HttpClient.Exceptions.Core;

namespace TestConsole.Netsmart;

internal class CareFabricErrorDetectorMessageHandler : DelegatingHandler
{
    private readonly IHttpClientConfiguration _config;

    public CareFabricErrorDetectorMessageHandler(IHttpClientConfiguration config)
    {
        _config = config;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (IsError(response))
            throw await CreateExceptionAsync(response, cancellationToken);

        return response;
    }

    private async Task<HttpClientException> CreateExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var context = await HttpClientErrorContext.CreateInstance(_config, response, cancellationToken);
        var executionStatus = response.GetHeaderValue(CareFabricConstants.StatusCodeHeader).ToCareFabricExecutionStatus();
        var message = response.GetHeaderValue(CareFabricConstants.StatusNoteHeader);
        return new CareFabricClientException(context, message, executionStatus);
    }

    private static bool IsError(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.OK)
            return true;

        var executionStatus = response.GetHeaderValue(CareFabricConstants.StatusCodeHeader).ToCareFabricExecutionStatus();
        var validStatus = new HashSet<CareFabricExecutionStatus>
        {
            CareFabricExecutionStatus.Success,
            CareFabricExecutionStatus.Warning
        };
        return !validStatus.Contains(executionStatus);
    }
}
