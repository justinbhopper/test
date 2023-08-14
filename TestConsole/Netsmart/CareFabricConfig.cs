using RH.Polaris.Options;

namespace TestConsole.Netsmart;

[BindConfiguration(@"carefabric")]
public sealed class CareFabricConfig
{
    public string SdkUrl { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string ScopeId { get; set; } = string.Empty;

    public string? SdkVersion { get; set; }

    public string? UserId { get; set; }

    public string InstanceId { get; set; } = string.Empty;

    public bool EnableEncryption { get; set; }
}
