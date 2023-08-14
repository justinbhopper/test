using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Netsmart.Bedrock.CareFabric.Core.Rigging;
using Netsmart.Bedrock.Rigging;

namespace TestConsole.Netsmart;

internal class CareFabricClient : ICareFabricClient
{
    private readonly CareFabricConfig _settings;
    private readonly HttpClient _client;
    private readonly IMessageEncryption _encryption;

    public CareFabricClient(
        IHttpClientFactory httpClientFactory,
        IOptions<CareFabricConfig> settings,
        EncryptionSettings encryptionSettings)
    {
        _settings = settings.Value;
        _client = httpClientFactory.CreateClient("carefabric");
        _encryption = new CareFabricMessageEncryption(settings.Value, encryptionSettings);
    }

    public async Task<CareFabricResponse> RequestAsync(IMessage message, CancellationToken cancellationToken)
    {
        var content = JsonSerializer.Serialize(message.Value, Serialization.Default);
        var timestamp = message.Timestamp.ToCareFabricUtcString();
        var mediaType = new MediaTypeWithQualityHeaderValue(CareFabricDo.ToMediaType(_settings.EnableEncryption));

        if (_settings.EnableEncryption)
            content = _encryption.Encrypt(message, content);

        var hash = _encryption.Hash(message, content);

        using var requestContent = new StringContent(content);
        requestContent.Headers.ContentType = mediaType;

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _settings.SdkUrl)
        {
            Content = requestContent,
        };

        requestMessage.Headers.Accept.Add(mediaType);
        requestMessage.Headers.Add(CareFabricConstants.ScopeIDHeader, _settings.ScopeId);
        requestMessage.Headers.Add(CareFabricConstants.MessageIDHeader, message.MessageId);
        requestMessage.Headers.Add(CareFabricConstants.MessageNameHeader, message.MessageName);
        requestMessage.Headers.Add(CareFabricConstants.MessageTypeHeader, message.MessageType.ToString());
        requestMessage.Headers.Add(CareFabricConstants.SdkIDHeader, _settings.ClientId);
        requestMessage.Headers.Add(CareFabricConstants.VersionHeader, _settings.SdkVersion);
        requestMessage.Headers.Add(CareFabricConstants.UserIDHeader, _settings.UserId);
        requestMessage.Headers.Add(CareFabricConstants.InitiatedTimestampHeader, timestamp);
        requestMessage.Headers.Add(CareFabricConstants.InstanceIDHeader, _settings.InstanceId);
        requestMessage.Headers.Add(CareFabricConstants.TimestampHeader, timestamp);
        requestMessage.Headers.Add(CareFabricConstants.HashHeader, hash);

        var response = await _client.SendAsync(requestMessage, cancellationToken);

        return new CareFabricResponse(response, _encryption, message);
    }
}
