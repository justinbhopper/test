using System.Text.Json;

namespace TestConsole.Netsmart;

internal class JsonInt32Converter : IJsonNumberConverter<int>
{
    public int Parse(string value)
    {
        return Convert.ToInt32(value);
    }

    public int Read(ref Utf8JsonReader reader)
    {
        return reader.GetInt32();
    }

    public void Write(Utf8JsonWriter writer, int value)
    {
        writer.WriteNumberValue(value);
    }
}

