using System.Text.Json;

namespace TestConsole.Netsmart;

internal class JsonInt16Converter : IJsonNumberConverter<short>
{
    public short Parse(string value)
    {
        return Convert.ToInt16(value);
    }

    public short Read(ref Utf8JsonReader reader)
    {
        return reader.GetInt16();
    }

    public void Write(Utf8JsonWriter writer, short value)
    {
        writer.WriteNumberValue(value);
    }
}

