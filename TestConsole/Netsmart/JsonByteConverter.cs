using System.Text.Json;

namespace TestConsole.Netsmart;

internal class JsonByteConverter : IJsonNumberConverter<byte>
{
    public byte Parse(string value)
    {
        return Convert.ToByte(value);
    }

    public byte Read(ref Utf8JsonReader reader)
    {
        return reader.GetByte();
    }

    public void Write(Utf8JsonWriter writer, byte value)
    {
        writer.WriteNumberValue(value);
    }
}

