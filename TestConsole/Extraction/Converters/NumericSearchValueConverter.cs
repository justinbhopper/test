namespace RH.Apollo.Persistence.Search.Converters;

public class NumericSearchValueConverter : ISearchValueConverter
{
    private static readonly ICollection<Type> s_numericTypes = new[]
    {
        typeof(int),
        typeof(short),
        typeof(long),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(byte),
        typeof(uint),
        typeof(ushort),
        typeof(ulong)
    };

    public bool CanConvert(Type type)
    {
        return s_numericTypes.Contains(type);
    }

    public IEnumerable<string> Convert(object value)
    {
        if (value != null)
            yield return value.ToString() ?? string.Empty;
    }
}
