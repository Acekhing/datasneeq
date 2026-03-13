using System.Text.Json;
using System.Text.RegularExpressions;
using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Transformations;

public class TransformationEngine : ITransformationEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public object? Transform(string? rawValue, ColumnMapping mapping, string? targetDataType)
    {
        var type = mapping.TransformationType ?? TransformationTypes.None;
        if (type == TransformationTypes.None)
            return rawValue;

        var configJson = mapping.TransformationConfigJson;
        if (string.IsNullOrEmpty(configJson) && mapping.TransformationConfig.HasValue)
            configJson = mapping.TransformationConfig.Value.GetRawText();

        return type switch
        {
            TransformationTypes.StringToBoolean => ApplyStringToBoolean(rawValue, configJson),
            TransformationTypes.ListPickFirst => ApplyListPickFirst(rawValue, configJson),
            _ => rawValue
        };
    }

    private static object? ApplyStringToBoolean(string? rawValue, string? configJson)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        var config = string.IsNullOrEmpty(configJson)
            ? new StringToBooleanConfig()
            : JsonSerializer.Deserialize<StringToBooleanConfig>(configJson, JsonOptions);

        if (config?.Mappings == null || config.Mappings.Count == 0)
            return null;

        var trimmed = rawValue.Trim();
        foreach (var m in config.Mappings)
        {
            if (string.Equals(m.ExcelValue, trimmed, StringComparison.OrdinalIgnoreCase))
                return m.BooleanValue;
        }

        return config.UseDefaultWhenNoMatch ? config.DefaultValue : (object?)null;
    }

    private static object? ApplyListPickFirst(string? rawValue, string? configJson)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        // Split by comma, semicolon, pipe, or line break (regex: [,;|\r\n]+)
        var parts = Regex.Split(rawValue.Trim(), @"[,;|\r\n]+");
        var first = parts.Select(p => p.Trim()).FirstOrDefault(p => !string.IsNullOrEmpty(p));
        return first ?? (object?)null;
    }
}
