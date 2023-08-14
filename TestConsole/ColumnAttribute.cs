namespace TestConsole;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ColumnAttribute : Attribute
{
    public ColumnAttribute(int ordinal)
    {
        Ordinal = ordinal;
    }

    public int Ordinal { get; }
}


