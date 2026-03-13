using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataSneeq.Domain.Models;

public class ColumnMapping
{
    public string ExcelColumn { get; set; } = string.Empty;
    public string DatabaseColumn { get; set; } = string.Empty;
    public bool AutoGenerate { get; set; }
    public string? TransformationType { get; set; }
    public string? TransformationConfigJson { get; set; }

    [JsonPropertyName("transformationConfig")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? TransformationConfig { get; set; }
}
