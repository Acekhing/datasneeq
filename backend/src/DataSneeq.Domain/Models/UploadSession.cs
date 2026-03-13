namespace DataSneeq.Domain.Models;

public class UploadSession
{
    public string FileId { get; set; } = string.Empty;
    public ExcelFileData? ExcelData { get; set; }
    public string? ConnectionString { get; set; }
    public string? SelectedTable { get; set; }
    public List<ColumnMapping>? Mappings { get; set; }
    public List<LookupRule>? LookupRules { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
}
