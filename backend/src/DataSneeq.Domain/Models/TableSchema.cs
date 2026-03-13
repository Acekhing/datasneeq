namespace DataSneeq.Domain.Models;

public class TableSchema
{
    public string SchemaName { get; set; } = "public";
    public string TableName { get; set; } = string.Empty;
    public List<ColumnSchema> Columns { get; set; } = new();
    public List<ForeignKeyInfo> ForeignKeys { get; set; } = new();
    public List<string> PrimaryKeys { get; set; } = new();
}
