using RH.Polaris.Options;

namespace TestConsole.OpenAi;

[BindConfiguration(@"openai")]
public sealed class OpenAiConfig
{
    public string Endpoint { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;
}
