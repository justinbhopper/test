using System.Security.Cryptography;
using Netsmart.Bedrock.CareFabric.Core.Rigging;
using Netsmart.Bedrock.Rigging;

namespace TestConsole.Netsmart;

internal class CareFabricMessageEncryption : IMessageEncryption
{
    private readonly CareFabricConfig _clientSettings;
    private readonly EncryptionSettings _encryptionSettings;

    public CareFabricMessageEncryption(CareFabricConfig clientSettings, EncryptionSettings encryptionSettings)
    {
        _clientSettings = clientSettings;
        _encryptionSettings = encryptionSettings;
    }

    public string Encrypt(IMessageContext context, string value)
    {
        value = context.MessageId + _encryptionSettings.Salt + value;

        var bytes = _encryptionSettings.Encoding.GetBytes(value);

        using var aes = CreateAlgorithm(context);

        using var encrypter = aes.CreateEncryptor(aes.Key, aes.IV);
        var encryptedBytes = encrypter.TransformFinalBlock(bytes, 0, bytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(IMessageContext context, string encryptedValue)
    {
        var bytes = Convert.FromBase64String(encryptedValue);

        using var aes = CreateAlgorithm(context);
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var decryptedBytes = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);

        var value = _encryptionSettings.Encoding.GetString(decryptedBytes);

        var prefix = context.MessageId + _encryptionSettings.Salt;
        return value.StartsWith(prefix) ? value[prefix.Length..] : value;
    }

    public string Hash(IMessageContext context, string value)
    {
        var timestamp = context.Timestamp.ToCareFabricUtcString();

        value = string.Join("|", _clientSettings.ClientId, _clientSettings.InstanceId, context.MessageId, timestamp, context.Method, string.Empty, value);

        // Take up to 6217 characters (don't know why this length)
        value = value[..Math.Min(6217, value.Length)];

        var bytes = _encryptionSettings.Encoding.GetBytes(value);
        var key = timestamp[..timestamp.IndexOf("T", StringComparison.Ordinal)] + _clientSettings.ClientSecret;

        using var hasher = new HMACSHA256(_encryptionSettings.Encoding.GetBytes(key));
        hasher.Initialize();

        var hash = hasher.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private SymmetricAlgorithm CreateAlgorithm(IMessageContext context)
    {
        var key = (context.MessageId[..Math.Min(context.MessageId.Length, 8)] + _encryptionSettings.SymmetricKey)[..32];

        var aes = Aes.Create();

        aes.Key = _encryptionSettings.Encoding.GetBytes(key);
        aes.IV = _encryptionSettings.Encoding.GetBytes(_encryptionSettings.InitializationVector);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.ISO10126;

        return aes;
    }
}
