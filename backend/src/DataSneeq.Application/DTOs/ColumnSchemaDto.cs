namespace DataSneeq.Application.DTOs;

public class ColumnSchemaDto
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool HasDefaultValue { get; set; }
    public int? MaxLength { get; set; }
    public bool IsForeignKey { get; set; }
}
