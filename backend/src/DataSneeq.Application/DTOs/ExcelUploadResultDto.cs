namespace DataSneeq.Application.DTOs;

public class ExcelUploadResultDto
{
    public string FileId { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
    public int RowCount { get; set; }
    public List<Dictionary<string, string>> SampleRows { get; set; } = new();
    public string FileName { get; set; } = string.Empty;
}
