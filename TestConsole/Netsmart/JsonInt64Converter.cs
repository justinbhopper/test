using System.Text.Json;

namespace TestConsole.Netsmart;

internal class JsonInt64Converter : IJsonNumberConverter<long>
{
    public long Parse(string value)
    {
        return Convert.ToInt64(value);
    }

    public long Read(ref Utf8JsonReader reader)
    {
        return reader.GetInt64();
    }

    public void Write(Utf8JsonWriter writer, long value)
    {
        writer.WriteNumberValue(value);
    }
}

