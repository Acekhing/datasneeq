using DataSneeq.Application.Interfaces;
using DataSneeq.Domain.Enums;
using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Services;

public class ForeignKeyBuildService : IForeignKeyBuildService
{
    private readonly IDatabaseProvider _dbProvider;

    public ForeignKeyBuildService(IDatabaseProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }

    public async Task<(object? resolvedId, bool wasCreated)> BuildFromExcelAsync(
        string connectionString,
        IReadOnlyDictionary<string, string> excelRow,
        string referencedTable,
        string returnColumn,
        List<ColumnMapping> foreignTableMappings,
        PrimaryKeyGenerationStrategy pkStrategy = PrimaryKeyGenerationStrategy.Uuid,
        bool simulateOnly = false,
        IDbTransactionScope? scope = null,
        LookupRule? lookupRule = null)
    {
        if (foreignTableMappings == null || foreignTableMappings.Count == 0)
            return (null, false);

        var tableSchema = await _dbProvider.GetTableSchemaAsync(connectionString, referencedTable);
        var pkColumns = tableSchema.PrimaryKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (lookupRule?.BuildMatchColumns is { Count: > 0 } buildMatchCols && !simulateOnly)
        {
            var matchPairs = new List<(string Column, object? Value)>();
            var mappingByDb = foreignTableMappings.ToDictionary(m => m.DatabaseColumn, StringComparer.OrdinalIgnoreCase);
            foreach (var dbCol in buildMatchCols)
            {
                if (!mappingByDb.TryGetValue(dbCol, out var mapping))
                    continue;
                excelRow.TryGetValue(mapping.ExcelColumn, out var rawValue);
                var dbColMeta = tableSchema.Columns.FirstOrDefault(c => c.Name.Equals(dbCol, StringComparison.OrdinalIgnoreCase));
                var converted = ConvertValue(rawValue, dbColMeta?.DataType);
                matchPairs.Add((dbCol, converted));
            }
            if (matchPairs.Count > 0)
            {
                object? existingId;
                if (scope != null)
                    existingId = await scope.LookupByCompositeAsync(referencedTable, matchPairs, returnColumn);
                else
                    existingId = await _dbProvider.LookupByCompositeAsync(connectionString, referencedTable, matchPairs, returnColumn);
                if (existingId != null && existingId != DBNull.Value)
                    return (existingId, false);
            }
        }

        var columns = new List<string>();
        var values = new List<object?>();

        if (pkStrategy == PrimaryKeyGenerationStrategy.Uuid && pkColumns.Count == 1)
        {
            var pkColName = tableSchema.PrimaryKeys[0];
            var pkCol = tableSchema.Columns.FirstOrDefault(c =>
                c.Name.Equals(pkColName, StringComparison.OrdinalIgnoreCase));
            if (pkCol != null)
            {
                var dt = pkCol.DataType?.ToLower() ?? "";
                // Generate PK for uuid (native) or text-like columns (store UUID string)
                if (dt == "uuid" || dt == "text" || dt == "character varying" || dt == "varchar")
                {
                    object pkValue = dt == "uuid" ? Guid.NewGuid() : Guid.NewGuid().ToString();
                    columns.Add(pkCol.Name);
                    values.Add(pkValue);
                }
            }
        }

        foreach (var mapping in foreignTableMappings)
        {
            if (pkColumns.Contains(mapping.DatabaseColumn))
                continue;

            excelRow.TryGetValue(mapping.ExcelColumn, out var rawValue);
            var dbCol = tableSchema.Columns.FirstOrDefault(c =>
                c.Name.Equals(mapping.DatabaseColumn, StringComparison.OrdinalIgnoreCase));
            var converted = ConvertValue(rawValue, dbCol?.DataType);
            columns.Add(mapping.DatabaseColumn);
            values.Add(converted);
        }

        if (columns.Count == 0)
            return (null, false);

        if (simulateOnly)
            return ("(preview)", true);

        object newId;
        if (scope != null)
        {
            newId = await scope.InsertSingleRecordAsync(referencedTable, columns, values, returnColumn);
        }
        else
        {
            newId = await _dbProvider.InsertSingleRecordAsync(
                connectionString, referencedTable, columns, values, returnColumn);
        }

        return (newId, true);
    }

    private static object? ConvertValue(string? value, string? dataType)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (string.IsNullOrEmpty(dataType)) return value;

        var normalized = dataType.ToLower();

        if (normalized.Contains("int"))
        {
            if (normalized == "bigint") return long.TryParse(value, out var l) ? l : (object)value;
            if (normalized == "smallint") return short.TryParse(value, out var s) ? s : (object)value;
            return int.TryParse(value, out var i) ? i : (object)value;
        }
        if (normalized is "numeric" or "decimal" or "money")
            return decimal.TryParse(value, out var d) ? d : (object)value;
        if (normalized is "real" or "double precision")
            return double.TryParse(value, out var dbl) ? dbl : (object)value;
        if (normalized == "boolean")
            return value is "1" or "true" or "True" or "TRUE" or "yes" or "Yes";
        if (normalized == "date" || normalized.Contains("timestamp"))
            return DateTime.TryParse(value, out var dt) ? dt : (object)value;
        if (normalized == "uuid")
            return Guid.TryParse(value, out var g) ? g : (object)value;

        return value;
    }
}
