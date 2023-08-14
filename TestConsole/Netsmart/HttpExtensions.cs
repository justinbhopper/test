using Netsmart.Bedrock.CareFabric.Core.Enumerations;
using Netsmart.Bedrock.CareFabric.Core.Rigging;

namespace TestConsole.Netsmart;

internal static class HttpExtensions
{
    public static string? GetHeaderValue(this HttpResponseMessage response, string name)
    {
        if (!response.Headers.TryGetValues(name, out var values))
            return null;

        return values.FirstOrDefault();
    }

    public static CareFabricExecutionStatus GetCareFabricExecutionStatus(this HttpResponseMessage response)
    {
        return response.GetHeaderValue(CareFabricConstants.StatusCodeHeader).ToCareFabricExecutionStatus();
    }

    public static bool IsTransientErrorStatus(this HttpResponseMessage response)
    {
        return response.GetCareFabricExecutionStatus().IsTransient();
    }

    public static bool IsTransient(this CareFabricExecutionStatus executionStatus)
    {
        return executionStatus switch
        {
            CareFabricExecutionStatus.Overloaded
            or CareFabricExecutionStatus.BusinessException
            or CareFabricExecutionStatus.Timeout
            or CareFabricExecutionStatus.Rejected
            or CareFabricExecutionStatus.Exception => true,
            _ => false,
        };
    }
}
