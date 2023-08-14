namespace System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class TableTypeAttribute : Attribute
{
    public TableTypeAttribute(string tableTypeName)
    {
        TableTypeName = tableTypeName;
    }

    public string TableTypeName { get; }
}
