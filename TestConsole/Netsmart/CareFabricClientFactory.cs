using System.Text;
using Microsoft.Extensions.Options;
using Netsmart.Bedrock.Rigging;

namespace TestConsole.Netsmart;

internal class CareFabricClientFactory : ICareFabricClientFactory
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IOptions<CareFabricConfig> _settings;

    public CareFabricClientFactory(IHttpClientFactory clientFactory, IOptions<CareFabricConfig> settings)
    {
        _clientFactory = clientFactory;
        _settings = settings;
    }

    public ICareFabricClient Create()
    {
        var encryptionSettings = new EncryptionSettings(Encoding.UTF8, "0000000000000000", "200AC6D037590B5E2F0DF6ED55CCED65", "56971C9E9C62B2AD40D43DCA3FC50AB0");
        return new CareFabricClient(_clientFactory, _settings, encryptionSettings);
    }
}
