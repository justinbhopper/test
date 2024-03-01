using System.Security.Cryptography;
using System.Text;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;

namespace TestConsole.KeyVaults;

public class KeyVaultTest
{
    public KeyVaultTest()
    {
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        using var rsa = RSA.Create();
        Console.WriteLine(rsa.ToXmlString(true));

        var azureCredential = new DefaultAzureCredential(true);

        //get key
        var KeyVaultName = "rh-kv-development";
        var keyClient = new KeyClient(new Uri($"https://{KeyVaultName}.vault.azure.net/"), azureCredential); ;
        var keyName = "test-key-1";
        var key = await keyClient.GetKeyAsync(keyName, "602455eabbde42f8a7a3501b43565aae", cancellationToken: cancellationToken);

        Console.WriteLine("The current key version is " + key.Value.Id);

        // create CryptographyClient
        var cryptoClient = new CryptographyClient(key.Value.Id, azureCredential);

        var str = "test";
        Console.WriteLine("The String used to be encrypted is :  " + str);

        Console.WriteLine("-------------encrypt---------------");
        var byteData = Encoding.Unicode.GetBytes(str);
        var encryptResult = await cryptoClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, byteData, cancellationToken);
        var encodedText = Convert.ToBase64String(encryptResult.Ciphertext);
        Console.WriteLine(encodedText);

        Console.WriteLine("-------------dencrypt---------------");
        var encryptedBytes = Convert.FromBase64String(encodedText);
        var dencryptResult = await cryptoClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, encryptedBytes, cancellationToken);
        var decryptedText = Encoding.Unicode.GetString(dencryptResult.Plaintext);
        Console.WriteLine(decryptedText);
    }
}
