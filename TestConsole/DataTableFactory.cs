using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;

namespace TestConsole;

public static class DataTableFactory
{
    private static readonly Dictionary<Type, RecordMeta> s_meta = new();

    public static DataTable Create<T>(IEnumerable<T> items)
        where T : class, new()
    {
        var meta = GetOrCreateMeta<T>();
        var values = Consolidate(meta, items);

        var table = new DataTable(meta.TableTypeName);

        foreach (var property in meta.Properties)
        {
            table.Columns.Add(property.Name, property.DataColumnType);
        }

        foreach (var value in values)
        {
            var row = table.NewRow();

            foreach (var property in meta.Properties)
            {
                row.SetField(property.Name, value[property.Name]);
            }

            table.Rows.Add(row);
        }

        return table;
    }

    private static IEnumerable<KeyValueCollection> Consolidate<T>(RecordMeta meta, IEnumerable<T> items)
        where T : class, new()
    {
        var values = items.Select(GetValues).ToList();
        var consoliated = new List<KeyValueCollection>();

        var grouped = values.GroupBy(GetGroupKey, new KeyValueCollection.KeyValueCollectionComparer()).ToList();

        foreach (var group in grouped)
        {
            var row = new KeyValueCollection();

            foreach (var key in group.Key)
            {
                row.Add(key.Key, key.Value);
            }

            foreach (var property in meta.Properties)
            {
                // Already added keys from group
                if (property.IsKey)
                    continue;

                var propertyValue = row.ContainsKey(property.Name) ? row[property.Name] : null;

                foreach (var item in group)
                {
                    var newPropertyValue = item[property.Name];

                    switch (property.Aggregation)
                    {
                        case PropertyAggregation.Sum:
                            propertyValue = Add(propertyValue, newPropertyValue);
                            break;
                        case PropertyAggregation.Max:
                            if (IsGreaterThan(newPropertyValue, propertyValue))
                                propertyValue = newPropertyValue;
                            break;
                    }
                }

                row[property.Name] = propertyValue;
            }

            consoliated.Add(row);
        }

        return consoliated;

        KeyValueCollection GetValues(T item)
        {
            var keys = new KeyValueCollection();

            foreach (var property in meta.Properties)
            {
                keys.Add(property.Name, property.GetValue(item));
            }

            return keys;
        }

        KeyValueCollection GetGroupKey(KeyValueCollection values)
        {
            var keys = new KeyValueCollection();

            foreach (var key in meta.Properties.Where(p => p.IsKey))
            {
                keys.Add(key.Name, values[key.Name]);
            }

            return keys;
        }
    }

    private static object? Add(object? left, object? right)
    {
        if (left is null)
            return right;

        if (right is null)
            return left;

        return left switch
        {
            int leftValue => leftValue + (int)right,
            short leftValue => leftValue + (short)right,
            long leftValue => leftValue + (long)right,
            byte leftValue => leftValue + (byte)right,
            float leftValue => leftValue + (float)right,
            double leftValue => leftValue + (double)right,
            decimal leftValue => leftValue + (decimal)right,
            char leftValue => leftValue + (char)right,
            uint leftValue => leftValue + (uint)right,
            ulong leftValue => leftValue + (ulong)right,
            ushort leftValue => leftValue + (ushort)right,
            sbyte leftValue => leftValue + (sbyte)right,
            _ => throw new InvalidOperationException($"Cannot perform summation on type '{left.GetType().Name}'."),
        };
    }

    private static bool IsGreaterThan(object? value, object? comparedTo)
    {
        var comparedToComparable = comparedTo as IComparable;

        if (value is not IComparable valueComparable)
            return comparedToComparable is not null;

        return valueComparable.CompareTo(comparedToComparable) > 0;
    }

    private static RecordMeta GetOrCreateMeta<T>()
    {
        var type = typeof(T);

        if (s_meta.TryGetValue(type, out var meta))
            return meta;

        var tableTypeName = type.GetCustomAttribute<TableTypeAttribute>(true)?.TableTypeName
            ?? GetDefaultTableTypeName(type);

        meta = new RecordMeta(tableTypeName);

        var properties = new Dictionary<int, PropertyMeta>();

        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            // Must have Column attribute
            var column = property.GetCustomAttribute<ColumnAttribute>(true);
            if (column is null)
                continue;

            var isKey = property.GetCustomAttribute<KeyAttribute>(true) != null;
            var sum = property.GetCustomAttribute<SumAttribute>(true) != null;

            var propertyMeta = new PropertyMeta(property)
            {
                IsKey = isKey,
                Aggregation = isKey ? null : sum ? PropertyAggregation.Sum : PropertyAggregation.Max
            };

            if (properties.ContainsKey(column.Ordinal))
                throw new InvalidOperationException($"Record {typeof(T).Name} is had multiple ColumnAttribute ordinal {column.Ordinal}.");

            properties[column.Ordinal] = propertyMeta;
        }

        var minOrdinal = properties.Keys.Min(); // Could start from zero or 1, as long as the rest don't have gaps
        for (var pos = 0; pos < properties.Count; ++pos)
        {
            var ordinal = pos + minOrdinal;
            if (!properties.ContainsKey(ordinal))
                throw new InvalidOperationException($"Record {typeof(T).Name} is missing ColumnAttribute ordinal {ordinal}.");

            meta.Properties.Add(properties[pos + minOrdinal]);
        }

        return meta;
    }

    private static string GetDefaultTableTypeName(Type type)
    {
        if (type.Name.EndsWith("Record"))
            return $"T_{type.Name[0..^6]}";
        return $"T_{type.Name}";
    }

    private class RecordMeta
    {
        public RecordMeta(string tableTypeName)
        {
            TableTypeName = tableTypeName;
        }

        public string TableTypeName { get; }

        public IList<PropertyMeta> Properties { get; } = new List<PropertyMeta>();
    }

    private class PropertyMeta
    {
        private readonly PropertyInfo _propertyInfo;

        public PropertyMeta(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
            DataColumnType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
        }

        public string Name => _propertyInfo.Name;

        public Type PropertyType => _propertyInfo.PropertyType;

        public Type DataColumnType { get; }

        public bool IsKey { get; set; }

        public PropertyAggregation? Aggregation { get; set; }

        public object? GetValue(object? obj)
        {
            return _propertyInfo.GetValue(obj);
        }
    }

    private enum PropertyAggregation
    {
        Max,
        Sum,
    }

    private class KeyValueCollection : Dictionary<string, object?>
    {
        private int _hashCode = int.MaxValue;

        public override int GetHashCode()
        {
            if (_hashCode != int.MaxValue)
                return _hashCode;

            int value;

            // Special case - if no keys, then assume no grouping should occur
            if (Count == 0)
            {
                value = base.GetHashCode();
            }
            else
            {
                var hashCode = new HashCode();
                foreach (var item in this)
                {
                    hashCode.Add(item.Value);
                }

                value = hashCode.ToHashCode();
            }

            Interlocked.Exchange(ref _hashCode, value);
            return value;
        }

        public class KeyValueCollectionComparer : IEqualityComparer<KeyValueCollection>
        {
            public bool Equals(KeyValueCollection? x, KeyValueCollection? y)
            {
                return x?.GetHashCode() == y?.GetHashCode();
            }

            public int GetHashCode([DisallowNull] KeyValueCollection obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
