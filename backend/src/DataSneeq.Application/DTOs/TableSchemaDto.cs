namespace DataSneeq.Application.DTOs;

public class TableSchemaDto
{
    public string SchemaName { get; set; } = "public";
    public string TableName { get; set; } = string.Empty;
    public List<ColumnSchemaDto> Columns { get; set; } = new();
    public List<ForeignKeyDto> ForeignKeys { get; set; } = new();
    public List<string> PrimaryKeys { get; set; } = new();
}
