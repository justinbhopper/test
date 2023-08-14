using Netsmart.Bedrock.CareFabric.Cdm.Entities;

namespace TestConsole.Netsmart;

public interface ICareFabricClient
{
    Task<CareFabricResponse> RequestAsync(IMessage message, CancellationToken cancellationToken);

    public async Task<TResponse> RequestAsync<TResponse>(IMessage message, CancellationToken cancellationToken)
    {
        var response = await RequestAsync(message, cancellationToken);
        return await response.ReadAsAsync<TResponse>(cancellationToken);
    }

    public IFeed<TResponse> GetFeed<TResponse>(IPayload payload)
        where TResponse : PayloadBase
    {
        return new CareFabricFeed<TResponse>(this, payload);
    }
}
