using System.Text.Json;

namespace TestConsole.Netsmart;

public interface IJsonNumberConverter<T>
    where T : struct, IConvertible
{
    T Parse(string value);

    T Read(ref Utf8JsonReader reader);

    void Write(Utf8JsonWriter writer, T value);
}
