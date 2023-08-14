namespace TestConsole.Netsmart;

public interface IMessageContext
{
    string MessageId { get; }

    string Method { get; }

    DateTimeOffset Timestamp { get; }
}
