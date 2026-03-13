using System.Text.Json.Serialization;
using DataSneeq.Domain.Enums;
using DataSneeq.Domain.Models;

namespace DataSneeq.Application.DTOs;

public class MappingConfigDto
{
    public string FileId { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<ColumnMapping> Mappings { get; set; } = new();
    public List<LookupRule> LookupRules { get; set; } = new();

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PrimaryKeyGenerationStrategy PrimaryKeyGenerationStrategy { get; set; } = PrimaryKeyGenerationStrategy.Uuid;

    /// <summary>Database columns that form the uniqueness key for duplicate detection. Empty = no check.</summary>
    public List<string> DuplicateKeyColumns { get; set; } = new();
}
