namespace DataSneeq.Application.DTOs;

public class ApiErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string? Code { get; set; }
    public object? Details { get; set; }
}
