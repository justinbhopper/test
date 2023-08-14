using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Netsmart.Bedrock.CareFabric.Core.Enumerations;
using Netsmart.Bedrock.CareFabric.Core.Rigging;

namespace TestConsole.Netsmart;

public sealed class CareFabricResponse : IDisposable
{
    private static readonly ISet<CareFabricExecutionStatus> s_validStatus = new HashSet<CareFabricExecutionStatus>
        {
            CareFabricExecutionStatus.Success,
            CareFabricExecutionStatus.Warning
        };

    private readonly HttpResponseMessage _response;
    private readonly IMessageEncryption _encryption;
    private readonly IMessageContext _encryptionContext;

    public CareFabricResponse(HttpResponseMessage response, IMessageEncryption encryption, IMessageContext encryptionContext)
    {
        _response = response;
        _encryption = encryption;
        _encryptionContext = encryptionContext;
    }

    public CareFabricExecutionStatus ExecutionStatus => GetHeaderValue("X-NTST-CF-Status-Code").ToCareFabricExecutionStatus();

    public string StatusNote => GetHeaderValue("X-NTST-CF-Status-Note");

    public HttpStatusCode StatusCode => _response.StatusCode;

    public HttpResponseHeaders Headers => _response.Headers;

    public void Dispose()
    {
        _response.Dispose();
    }

    public void EnsureSuccessStatusCode()
    {
        _response.EnsureSuccessStatusCode();

        if (!s_validStatus.Contains(ExecutionStatus))
            throw new HttpRequestException($"ExecutionStatus {ExecutionStatus} was received.  {StatusNote}");
    }

    public async Task<T> ReadAsAsync<T>(CancellationToken cancellationToken)
    {
        var content = await ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(content, Serialization.Default)
            ?? throw new JsonException("Failed to deserialize.");
    }

    public async Task<string> ReadAsStringAsync(CancellationToken cancellationToken)
    {
        // Response body will not contain anything useful in case of failures
        EnsureSuccessStatusCode();

        var content = await _response.Content.ReadAsStringAsync(cancellationToken);

        var mediaType = _response.Content.Headers.ContentType?.MediaType;
        if (mediaType is null || !CareFabricDo.ToIsContentEncrypted(mediaType))
            return content;

        return _encryption.Decrypt(_encryptionContext, content);
    }

    private string GetHeaderValue(string name)
    {
        if (!Headers.TryGetValues(name, out var values))
            throw new InvalidOperationException($"Header '{name}' was not on the response.");

        return values.First();
    }
}
