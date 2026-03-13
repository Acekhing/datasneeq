namespace DataSneeq.Domain.Models;

public class BatchInsertResult
{
    public int InsertedCount { get; set; }
    public int SkippedCount { get; set; }
    public int LookupRecordsCreated { get; set; }
    public List<string> Warnings { get; set; } = new();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
