using Netsmart.Bedrock.CareFabric.Core.Enumerations;

namespace TestConsole.Netsmart;

public interface IMessage : IMessageContext
{
    string MessageName { get; }

    CareFabricMessageType MessageType { get; }

    object Value { get; }

    IMessage OnBehalfOf(string? requesterId);
}
