namespace DataSneeq.Application.DTOs;

public class UploadCommitResultDto
{
    public bool Success { get; set; }
    public int InsertedCount { get; set; }
    public int SkippedCount { get; set; }
    public int LookupRecordsCreated { get; set; }
    public List<string> Warnings { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
