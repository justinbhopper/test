using System.Diagnostics;

namespace RH.Apollo.Persistence.Search;

/// <summary>
///     Represents a search index entry.
/// </summary>
[DebuggerDisplay("ParamName={ParamName} Value={Value}")]
public class SearchIndexEntry : IEquatable<SearchIndexEntry>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SearchIndexEntry" /> class.
    /// </summary>
    /// <param name="paramName">The search parameter name.</param>
    /// <param name="value">The searchable value.</param>
    public SearchIndexEntry(string paramName, string? value, bool isUnique)
    {
        ParamName = paramName;
        Value = value;
        IsUnique = isUnique;
    }

    /// <summary>
    ///     Gets the parameter name.
    /// </summary>
    public string ParamName { get; }

    /// <summary>
    ///     Gets the searchable value.
    /// </summary>
    public string? Value { get; }

    public bool IsUnique { get; }

    public bool Equals([NotNullWhen(true)] SearchIndexEntry? other)
    {
        if (other is null)
            return false;

        if (!string.Equals(ParamName, other.ParamName, StringComparison.OrdinalIgnoreCase))
            return false;

        return string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public override bool Equals([NotNullWhen(true)] object? other)
    {
        if (other is null)
            return false;

        return other is SearchIndexEntry value && Equals(value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ParamName, Value);
    }
}
