using System.Text.Json;
using System.Text.Json.Serialization;
using Netsmart.Bedrock.Rigging;

namespace TestConsole.Netsmart;

public static class Serialization
{
    static Serialization()
    {
        Default = Do.CreateDefaultJsonSerializerOptions();

        // Forgiving string converters
        Default.Converters.Decorate<string>(d => new ForgivingStringConverter(d));

        // Forgiving number converters
        Default.Converters.Decorate<byte>(d => new ForgivingNumberConverter<byte>(d, new JsonByteConverter()));
        Default.Converters.Decorate<short>(d => new ForgivingNumberConverter<short>(d, new JsonInt16Converter()));
        Default.Converters.Decorate<int>(d => new ForgivingNumberConverter<int>(d, new JsonInt32Converter()));
        Default.Converters.Decorate<long>(d => new ForgivingNumberConverter<long>(d, new JsonInt64Converter()));
        Default.Converters.Decorate<float>(d => new ForgivingNumberConverter<float>(d, new JsonSingleConverter()));
        Default.Converters.Decorate<double>(d => new ForgivingNumberConverter<double>(d, new JsonDoubleConverter()));
        Default.Converters.Decorate<decimal>(d => new ForgivingNumberConverter<decimal>(d, new JsonDecimalConverter()));
        Default.Converters.Decorate<byte?>(d => new ForgivingNullableNumberConverter<byte>(d, new JsonByteConverter()));
        Default.Converters.Decorate<short?>(d => new ForgivingNullableNumberConverter<short>(d, new JsonInt16Converter()));
        Default.Converters.Decorate<int?>(d => new ForgivingNullableNumberConverter<int>(d, new JsonInt32Converter()));
        Default.Converters.Decorate<long?>(d => new ForgivingNullableNumberConverter<long>(d, new JsonInt64Converter()));
        Default.Converters.Decorate<float?>(d => new ForgivingNullableNumberConverter<float>(d, new JsonSingleConverter()));
        Default.Converters.Decorate<double?>(d => new ForgivingNullableNumberConverter<double>(d, new JsonDoubleConverter()));
        Default.Converters.Decorate<decimal?>(d => new ForgivingNullableNumberConverter<decimal>(d, new JsonDecimalConverter()));
    }

    public static readonly JsonSerializerOptions Default;
}

internal static class SerializationExtensions
{
    public static void Decorate<T>(this IList<JsonConverter> converters, Func<JsonConverter<T>?, JsonConverter<T>> factory)
    {
        var decorated = converters.FirstOrDefault(c => c is JsonConverter<T>);
        if (decorated != null)
            converters.Remove(decorated);

        converters.Add(factory(decorated as JsonConverter<T>));
    }
}
