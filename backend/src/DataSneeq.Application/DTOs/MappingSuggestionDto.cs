namespace DataSneeq.Application.DTOs;

public class MappingSuggestionDto
{
    public string ExcelColumn { get; set; } = string.Empty;
    public string? SuggestedDbColumn { get; set; }
    public double Confidence { get; set; }
}
