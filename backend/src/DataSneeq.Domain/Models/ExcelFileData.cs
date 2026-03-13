namespace DataSneeq.Domain.Models;

public class ExcelFileData
{
    public string FileId { get; set; } = string.Empty;
    public List<string> Headers { get; set; } = new();
    public List<Dictionary<string, string>> Rows { get; set; } = new();
    public int RowCount => Rows.Count;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
