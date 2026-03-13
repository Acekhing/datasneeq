namespace DataSneeq.Domain.Models;

public class ColumnSchema
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool HasDefaultValue { get; set; }
    public string? DefaultValue { get; set; }
    public int? MaxLength { get; set; }
    public int? NumericPrecision { get; set; }
    public bool IsForeignKey { get; set; }
}
