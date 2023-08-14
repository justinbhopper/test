namespace TestConsole.JsonTest;

public class JpegEncoding : ImageEncoding
{
    public override ImageType ImageType => ImageType.Jpeg;

    /// <summary>
    /// Must be between 0 and 100.  0 is highest compression, and 100 is highest quality.
    /// </summary>
    public int Quality { get; set; } = 100;
}
