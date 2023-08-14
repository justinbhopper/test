namespace TestConsole.JsonTest;

public class BlobResizeInstructions : ResizeInstructions
{
    public BlobResizeInstructions(string destinationBlobName)
    {
        DestinationBlobName = destinationBlobName;
    }

    public string DestinationBlobName { get; set; }
}
