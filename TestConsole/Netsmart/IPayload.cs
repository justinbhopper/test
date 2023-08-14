using Netsmart.Bedrock.CareFabric.Cdm.Entities;

namespace TestConsole.Netsmart;

public interface IPayload : IMessageContext, IMessage
{
    new PayloadBase Value { get; }

    new IPayload OnBehalfOf(string? requesterId);
}
