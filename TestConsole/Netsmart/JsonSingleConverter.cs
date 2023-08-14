using System.Text.Json;

namespace TestConsole.Netsmart;

internal class JsonSingleConverter : IJsonNumberConverter<float>
{
    public float Parse(string value)
    {
        return Convert.ToSingle(value);
    }

    public float Read(ref Utf8JsonReader reader)
    {
        return reader.GetSingle();
    }

    public void Write(Utf8JsonWriter writer, float value)
    {
        writer.WriteNumberValue(value);
    }
}

