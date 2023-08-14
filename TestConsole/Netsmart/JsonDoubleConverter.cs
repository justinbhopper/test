using System.Text.Json;

namespace TestConsole.Netsmart;

internal class JsonDoubleConverter : IJsonNumberConverter<double>
{
    public double Parse(string value)
    {
        return Convert.ToDouble(value);
    }

    public double Read(ref Utf8JsonReader reader)
    {
        return reader.GetDouble();
    }

    public void Write(Utf8JsonWriter writer, double value)
    {
        writer.WriteNumberValue(value);
    }
}

