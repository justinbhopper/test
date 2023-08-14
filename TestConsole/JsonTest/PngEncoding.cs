namespace TestConsole.JsonTest;

public class PngEncoding : ImageEncoding
{
    public override ImageType ImageType => ImageType.Png;

    public bool AllowTransparency { get; set; } = true;
}
