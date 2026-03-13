namespace DataSneeq.Application.DTOs;

public class MappingSuggestRequestDto
{
    public string FileId { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
}
