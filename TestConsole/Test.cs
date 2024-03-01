using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RH.Apollo.Contracts.Resources;
using RH.Apollo.Contracts.Serialization;
using RH.Polaris.Host.Cli.Abstract;
using RH.Polaris.NewtonsoftJson;
using TestConsole.JsonTest;
using TestConsole.KeyVaults;
using TestConsole.OpenAi;
using DescriptionAttribute = Oakton.DescriptionAttribute;

namespace TestConsole;

[Description("Test", Name = "test")]
public sealed class TestCommand : HostedAsyncCommand
{
    protected override async Task<bool> Execute(IServiceProvider app, CancellationToken cancellationToken)
    {
        GenerateSig();

        //AthenaTest();
        //SigTest();
        //await OpenAiTest(app, cancellationToken);
        //await NetsmartTest(app, cancellationToken);
        //await KeyVaultTest(app, cancellationToken);
        //GenerateSig();
        //SigTest();

        await Task.CompletedTask;
        return true;
    }

    public static string Bracketize(string path)
    {
        var sb = new StringBuilder();

        var dot = path.IndexOf('.');
        var startBracket = path.IndexOf('[');

        while (dot != -1 || startBracket != -1)
        {
            int skip;
            if (startBracket != -1 && (dot == -1 || startBracket < dot))
            {
                var endBracket = path.IndexOf(']');
                if (endBracket == -1)
                    throw new InvalidOperationException("Invalid bracket notation");

                skip = endBracket + 1;
                if (startBracket > 0)
                    AppendBracketed(path[0..startBracket]);
                sb.Append(path[startBracket..skip]);
            }
            else
            {
                skip = dot + 1;
                if (dot > 0)
                    AppendBracketed(path[0..dot]);
            }

            if (path.Length <= skip)
            {
                path = string.Empty;
                break;
            }

            path = path[skip..];
            dot = path.IndexOf('.');
            startBracket = path.IndexOf('[');
        }

        if (!string.IsNullOrEmpty(path))
            AppendBracketed(path);

        return sb.ToString();

        void AppendBracketed(string path)
        {
            if (sb.Length > 0)
                sb.Append("['").Append(path).Append("']");
            else
                sb.Append(path);
        }
    }

    private async Task KeyVaultTest(IServiceProvider app, CancellationToken cancellationToken)
    {
        var test = app.GetRequiredService<KeyVaultTest>();
        await test.Execute(cancellationToken);
    }

    private async Task OpenAiTest(IServiceProvider app, CancellationToken cancellationToken)
    {
        var test = app.GetRequiredService<OpenAiTest>();
        await test.Test(cancellationToken);
    }

    private void DeserializeTest()
    {
        var json = File.ReadAllText(@"F:\Workarea\Sandboxes\testconsole\test2.json");

        for (var i = 0; i < 20; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = JsonConvert.DeserializeObject<IList<Patient>>(json, new DefaultSerializerSettings());
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }
    }
    /*
    private void SigTest()
    {
        //var sig = "AAoAAAAAAAAAAAAAAAAAAAAozBAAAAAAAAACWAfQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACJUE5HDQoaCgAAAA1JSERSAAAH0AAAAlgIAgAAALWB6jwAABMqSURBVHic7djBCQAhEMDA8/rveS3CgCAzFeSdNTMfAAAAAABw5r8dAAAAAAAALzDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAEDHcAAAAAAAgY7gAAAAAAEDDcAQAAAAAgYLgDAAAAAEDAcAcAAAAAgIDhDgAAAAAAAcMdAAAAAAAChjsAAAAAAAQMdwAAAAAACBjuAAAAAAAQMNwBAAAAACBguAMAAAAAQMBwBwAAAACAgOEOAAAAAAABwx0AAAAAAAKGOwAAAAAABAx3AAAAAAAIGO4AAAAAABAw3AEAAAAAIGC4AwAAAABAwHAHAAAAAICA4Q4AAAAAAAHDHQAAAAAAAoY7AAAAAAAENkj8B62gEdmnAAAAAElFTkSuQmCC";
        var sig = "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";
        var sigBytes = Convert.FromBase64String(sig);
        //sigBytes = sigBytes.Skip(56).ToArray();
        Console.WriteLine($"IsAllWhite = {IsAllWhite(sigBytes)}");
        Console.WriteLine($"IsAllWhite = {IsAllWhite(File.ReadAllBytes(@"F:\Workarea\Sandboxes\test\testconsole\Bike.png"))}");
        File.WriteAllBytes(@"F:\Workarea\RemarkableHealth\test\testconsole\sig.png", sigBytes);
    }
    */
    private void GenerateSig()
    {
        int screenWidth = 1900;
        int screenHeight = 1000;

#pragma warning disable CA1416 // Validate platform compatibility
        byte[] originalBytes = File.ReadAllBytes(@"F:\large.jpg");
        using MemoryStream originalStream = new MemoryStream(originalBytes);
        Image originalImage = Image.FromStream(originalStream);

        //if (originalImage.Width <= screenWidth && originalImage.Height <= screenHeight)
        //    return originalBytes;

        if (originalImage.Width <= screenWidth)
            screenWidth = originalImage.Width;

        var newHeight = originalImage.Height * screenWidth / originalImage.Width;
        if (newHeight > screenHeight)
        {
            // Resize with height instead  
            screenWidth = originalImage.Width * screenHeight / originalImage.Height;
            newHeight = screenHeight;
        }

        using Image resizedImage = new Bitmap(screenWidth, newHeight);

        using (var graphic = Graphics.FromImage(resizedImage))
        {
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.SmoothingMode = SmoothingMode.HighQuality;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighQuality;
            graphic.Clear(Color.White);
            graphic.DrawImage(originalImage, 0, 0, screenWidth, newHeight);
        }

        using var outputStream = new MemoryStream();
        resizedImage.Save(outputStream, originalImage.RawFormat);
        File.WriteAllBytes(@"F:\resized.png", outputStream.ToArray());
#pragma warning restore CA1416 // Validate platform compatibility
    }

    private Font GetAutoscaledFont(Graphics graphics, string graphicString, Font originalFont, int containerWidth, int containerHeight, float maxFontSize, out System.Drawing.SizeF measuredSize)
    {
        // Reduce font size by 10 each attempt
        var stepSize = 10;

        // We utilize MeasureString which we get via a control instance
        for (var adjustedSize = maxFontSize; adjustedSize >= originalFont.Size; adjustedSize -= stepSize)
        {
            using var testFont = new Font(originalFont.Name, adjustedSize, originalFont.Style);

            // Test the string with the new size
            var adjustedSizeNew = graphics.MeasureString(graphicString, testFont);

            if (containerWidth > Convert.ToInt32(adjustedSizeNew.Width) && containerHeight > Convert.ToInt32(adjustedSizeNew.Height))
            {
                // Good font, return it
                measuredSize = adjustedSizeNew;
                return testFont;
            }
        }

        // If you get here there was no fontsize that worked
        measuredSize = graphics.MeasureString(graphicString, originalFont);
        return originalFont;
    }

    /*

    private bool IsAllWhite(byte[] data)
    {
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(data);

        var allWhite = true;
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                if (!allWhite)
                    break;

                var row = accessor.GetRowSpan(y);

                for (var x = 0; x < row.Length; x++)
                {
                    ref var pixel = ref row[x];
                    if (!pixel.Equals(SixLabors.ImageSharp.Color.White))
                    {
                        allWhite = false;
                        break;
                    }
                }
            }
        });

        return allWhite;
    }
    */
    private void JsonTest()
    {
        var settings = new JsonSerializerSettings();
        settings.ApplyPolarisDefaults();
        settings.Converters.Add(new ImageEncodingConverter());
        settings.Converters.Add(new StringEnumConverter());
        var instructions = new[]
        {
                new BlobResizeInstructions("destinationBlobName")
                {
                    Encoding = new PngEncoding { AllowTransparency = true },
                    Height = 200,
                    Width = 300,
                    PadColorHex = "FFF",
                }
            };
        var json = JsonConvert.SerializeObject(instructions, settings);
        Console.WriteLine(json);
        var deserialized = JsonConvert.DeserializeObject<IList<BlobResizeInstructions>>(json, settings);
        Console.WriteLine(deserialized);
    }
}
