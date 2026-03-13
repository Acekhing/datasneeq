using DataSneeq.Domain.Enums;

namespace DataSneeq.Domain.Models;

public class MappingTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string TargetTable { get; set; } = string.Empty;
    public string MappingsJson { get; set; } = "[]";
    public string LookupRulesJson { get; set; } = "[]";
    public string DuplicateKeyColumnsJson { get; set; } = "[]";
    public PrimaryKeyGenerationStrategy PrimaryKeyGenerationStrategy { get; set; } = PrimaryKeyGenerationStrategy.Uuid;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
