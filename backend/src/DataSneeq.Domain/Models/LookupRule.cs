using System.Text.Json.Serialization;
using DataSneeq.Domain.Enums;

namespace DataSneeq.Domain.Models;

public class LookupRule
{
    public string ForeignKeyColumn { get; set; } = string.Empty;
    public string LookupTable { get; set; } = string.Empty;
    public string LookupDisplayColumn { get; set; } = string.Empty;
    public bool AutoCreate { get; set; } = true;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ForeignKeyProcessingMode ProcessingMode { get; set; } = ForeignKeyProcessingMode.Lookup;

    public List<ColumnMapping> ForeignTableMappings { get; set; } = new();

    /// <summary>When Build-from-Excel: database column(s) to check for existing record. Empty = always create.</summary>
    public List<string> BuildMatchColumns { get; set; } = new();
}
