using DataSneeq.Domain.Enums;

namespace DataSneeq.Domain.Models;

public class ValidationError
{
    public int RowNumber { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ValidationErrorType ErrorType { get; set; }
    public string? Value { get; set; }
}
