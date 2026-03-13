using System.Text.Json.Serialization;
using DataSneeq.Domain.Enums;
using DataSneeq.Domain.Models;

namespace DataSneeq.Application.DTOs;

public class MappingTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TargetTable { get; set; } = string.Empty;
    public List<ColumnMapping> Mappings { get; set; } = new();
    public List<LookupRule> LookupRules { get; set; } = new();

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PrimaryKeyGenerationStrategy PrimaryKeyGenerationStrategy { get; set; } = PrimaryKeyGenerationStrategy.Uuid;

    public List<string> DuplicateKeyColumns { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveMappingTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string TargetTable { get; set; } = string.Empty;
    public List<ColumnMapping> Mappings { get; set; } = new();
    public List<LookupRule> LookupRules { get; set; } = new();

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PrimaryKeyGenerationStrategy PrimaryKeyGenerationStrategy { get; set; } = PrimaryKeyGenerationStrategy.Uuid;

    public List<string> DuplicateKeyColumns { get; set; } = new();
}
