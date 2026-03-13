namespace DataSneeq.Application.DTOs;

public class UploadPreviewDto
{
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public List<ValidationErrorDto> Errors { get; set; } = new();
    public List<LookupResolutionDto> LookupResolutions { get; set; } = new();
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int ErrorRows { get; set; }
}

public class ValidationErrorDto
{
    public int RowNumber { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string? Value { get; set; }
}

public class LookupResolutionDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string OriginalValue { get; set; } = string.Empty;
    public string LookupTable { get; set; } = string.Empty;
    public object? ResolvedId { get; set; }
    public bool WasCreated { get; set; }
    public string? ProcessingMode { get; set; }
    public Dictionary<string, object?>? ForeignRecordPreview { get; set; }
}
