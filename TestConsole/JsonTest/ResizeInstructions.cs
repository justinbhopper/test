namespace TestConsole.JsonTest;

public class ResizeInstructions
{
    public int Width { get; set; }

    public int Height { get; set; }

    public ImageEncoding Encoding { get; set; } = new JpegEncoding();

    /// <summary>
    /// If set, this color will be used to fill any remaining area.  If not set, the
    /// original aspect ratio of the image will be maintained.
    /// </summary>
    public string? PadColorHex { get; set; }
}
