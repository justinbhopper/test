using Netsmart.Bedrock.CareFabric.Core.Enumerations;
using RH.Polaris.HttpClient.Exceptions.Core;

namespace TestConsole.Netsmart;

public class CareFabricClientException : HttpClientException
{
    internal CareFabricClientException(HttpClientErrorContext context, string? message, CareFabricExecutionStatus executionStatus, Exception? innerException = null)
        : base(context, message, innerException)
    {
        ExecutionStatus = executionStatus;
    }

    public CareFabricExecutionStatus ExecutionStatus { get; }
}
