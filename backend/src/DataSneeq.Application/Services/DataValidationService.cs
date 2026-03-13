using DataSneeq.Application.Transformations;
using DataSneeq.Application.Interfaces;
using DataSneeq.Domain.Enums;
using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Services;

public class DataValidationService : IDataValidationService
{
    public List<ValidationError> ValidateRow(
        int rowNumber,
        Dictionary<string, string> row,
        List<ColumnMapping> mappings,
        TableSchema tableSchema)
    {
        var errors = new List<ValidationError>();

        foreach (var mapping in mappings)
        {
            var dbColumn = tableSchema.Columns.FirstOrDefault(c => c.Name == mapping.DatabaseColumn);
            if (dbColumn == null) continue;
            if (mapping.AutoGenerate) continue;

            row.TryGetValue(mapping.ExcelColumn, out var value);

            if (string.IsNullOrWhiteSpace(value))
            {
                if (!dbColumn.IsNullable && !dbColumn.HasDefaultValue && !dbColumn.IsPrimaryKey)
                {
                    errors.Add(new ValidationError
                    {
                        RowNumber = rowNumber,
                        ColumnName = mapping.ExcelColumn,
                        Message = $"{mapping.DatabaseColumn} is required",
                        ErrorType = ValidationErrorType.RequiredField,
                        Value = value
                    });
                }
                continue;
            }

            if (dbColumn.MaxLength.HasValue && value.Length > dbColumn.MaxLength.Value)
            {
                errors.Add(new ValidationError
                {
                    RowNumber = rowNumber,
                    ColumnName = mapping.ExcelColumn,
                    Message = $"Value exceeds maximum length of {dbColumn.MaxLength.Value}",
                    ErrorType = ValidationErrorType.ValueTooLong,
                    Value = value
                });
                continue;
            }

            var hasTransformation = !string.IsNullOrEmpty(mapping.TransformationType)
                && mapping.TransformationType != TransformationTypes.None;
            if (!hasTransformation)
            {
                var typeError = ValidateType(rowNumber, mapping.ExcelColumn, value, dbColumn.DataType);
                if (typeError != null)
                    errors.Add(typeError);
            }
        }

        return errors;
    }

    public List<ValidationError> ValidateBatchDuplicates(
        IReadOnlyList<Dictionary<string, string>> rows,
        List<ColumnMapping> mappings,
        IReadOnlyList<string> duplicateKeyColumns)
    {
        var errors = new List<ValidationError>();
        if (duplicateKeyColumns == null || duplicateKeyColumns.Count == 0)
            return errors;

        var mappingByDb = mappings.Where(m => !string.IsNullOrEmpty(m.DatabaseColumn) && !string.IsNullOrEmpty(m.ExcelColumn))
            .ToDictionary(m => m.DatabaseColumn, StringComparer.OrdinalIgnoreCase);

        var seenKeys = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNum = i + 2;
            var keyParts = new List<string>();
            var keyDisplay = new List<string>();
            foreach (var dbCol in duplicateKeyColumns)
            {
                if (!mappingByDb.TryGetValue(dbCol, out var mapping))
                    continue;
                row.TryGetValue(mapping.ExcelColumn, out var val);
                var part = val ?? "\0";
                keyParts.Add(part);
                keyDisplay.Add($"{dbCol}={val ?? "null"}");
            }
            if (keyParts.Count == 0)
                continue;
            var key = string.Join("\x1f", keyParts);
            if (seenKeys.TryGetValue(key, out var firstRowNum))
            {
                errors.Add(new ValidationError
                {
                    RowNumber = rowNum,
                    ColumnName = string.Join(", ", duplicateKeyColumns),
                    Message = $"Duplicate key (same as row {firstRowNum}): {string.Join(", ", keyDisplay)}",
                    ErrorType = ValidationErrorType.DuplicateRow,
                    Value = key
                });
            }
            else
            {
                seenKeys[key] = rowNum;
            }
        }
        return errors;
    }

    private static ValidationError? ValidateType(int rowNumber, string columnName, string value, string dataType)
    {
        var normalizedType = dataType.ToLower();

        if (normalizedType.Contains("int") || normalizedType == "smallint" || normalizedType == "bigint")
        {
            if (!long.TryParse(value, out _))
                return MakeError(rowNumber, columnName, value, "Value is not a valid integer", ValidationErrorType.InvalidNumber);
        }
        else if (normalizedType == "numeric" || normalizedType == "decimal" || normalizedType == "real"
                 || normalizedType == "double precision" || normalizedType == "money")
        {
            if (!decimal.TryParse(value, out _))
                return MakeError(rowNumber, columnName, value, "Value is not a valid number", ValidationErrorType.InvalidNumber);
        }
        else if (normalizedType == "date" || normalizedType.Contains("timestamp"))
        {
            if (!DateTime.TryParse(value, out _))
                return MakeError(rowNumber, columnName, value, "Value is not a valid date", ValidationErrorType.InvalidDate);
        }
        else if (normalizedType == "boolean")
        {
            if (!bool.TryParse(value, out _) && value != "0" && value != "1")
                return MakeError(rowNumber, columnName, value, "Value is not a valid boolean", ValidationErrorType.InvalidDataType);
        }
        else if (normalizedType == "uuid")
        {
            if (!Guid.TryParse(value, out _))
                return MakeError(rowNumber, columnName, value, "Value is not a valid UUID", ValidationErrorType.InvalidDataType);
        }

        return null;
    }

    private static ValidationError MakeError(int row, string col, string value, string msg, ValidationErrorType type)
    {
        return new ValidationError
        {
            RowNumber = row,
            ColumnName = col,
            Value = value,
            Message = msg,
            ErrorType = type
        };
    }
}
