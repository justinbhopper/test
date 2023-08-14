namespace TestConsole.Netsmart;

public interface IMessageEncryption
{
    string Hash(IMessageContext context, string value);

    string Encrypt(IMessageContext context, string value);

    string Decrypt(IMessageContext context, string value);
}
