using System.Text.Json;

namespace TestConsole.Netsmart;

internal class JsonDecimalConverter : IJsonNumberConverter<decimal>
{
    public decimal Parse(string value)
    {
        return Convert.ToDecimal(value);
    }

    public decimal Read(ref Utf8JsonReader reader)
    {
        return reader.GetDecimal();
    }

    public void Write(Utf8JsonWriter writer, decimal value)
    {
        writer.WriteNumberValue(value);
    }
}

