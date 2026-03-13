using DataSneeq.Application.DTOs;
using DataSneeq.Application.Interfaces;
using DataSneeq.Application.Transformations;
using DataSneeq.Domain.Enums;
using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Services;

public class UploadOrchestrationService : IUploadOrchestrationService
{
    private readonly IUploadSessionService _sessionService;
    private readonly IDatabaseProvider _dbProvider;
    private readonly IDataValidationService _validationService;
    private readonly IForeignKeyResolutionService _fkResolutionService;
    private readonly IForeignKeyBuildService _fkBuildService;
    private readonly ITransformationEngine _transformationEngine;

    public UploadOrchestrationService(
        IUploadSessionService sessionService,
        IDatabaseProvider dbProvider,
        IDataValidationService validationService,
        IForeignKeyResolutionService fkResolutionService,
        IForeignKeyBuildService fkBuildService,
        ITransformationEngine transformationEngine)
    {
        _sessionService = sessionService;
        _dbProvider = dbProvider;
        _validationService = validationService;
        _fkResolutionService = fkResolutionService;
        _fkBuildService = fkBuildService;
        _transformationEngine = transformationEngine;
    }

    public async Task<UploadPreviewDto> PreviewAsync(MappingConfigDto config)
    {
        var session = _sessionService.Get(config.FileId)
            ?? throw new InvalidOperationException("Upload session not found. Please upload the file again.");

        var excelData = session.ExcelData
            ?? throw new InvalidOperationException("No Excel data in session.");

        var tableSchema = await _dbProvider.GetTableSchemaAsync(config.ConnectionString, config.TableName);
        var result = new UploadPreviewDto { TotalRows = excelData.RowCount };
        var allErrors = new List<ValidationErrorDto>();
        var allResolutions = new List<LookupResolutionDto>();
        var previewRows = new List<Dictionary<string, object?>>();

        var fkMap = tableSchema.ForeignKeys.ToDictionary(fk => fk.ColumnName, StringComparer.OrdinalIgnoreCase);
        var (autoGenMappings, regularMappings) = SplitMappings(config.Mappings);

        var duplicateErrors = config.DuplicateKeyColumns is { Count: > 0 } dupCols
            ? _validationService.ValidateBatchDuplicates(excelData.Rows, config.Mappings, dupCols)
            : new List<ValidationError>();
        var duplicateRowNums = duplicateErrors.Select(e => e.RowNumber).ToHashSet();
        foreach (var err in duplicateErrors)
        {
            allErrors.Add(new ValidationErrorDto
            {
                RowNumber = err.RowNumber,
                ColumnName = err.ColumnName,
                Message = err.Message,
                ErrorType = err.ErrorType.ToString(),
                Value = err.Value
            });
        }

        for (int i = 0; i < excelData.Rows.Count; i++)
        {
            var row = excelData.Rows[i];
            int rowNum = i + 2; // 1-indexed, row 1 is headers

            var errors = _validationService.ValidateRow(rowNum, row, config.Mappings, tableSchema);
            var processedRow = new Dictionary<string, object?>();
            bool hasError = duplicateRowNums.Contains(rowNum) || errors.Count > 0;

            foreach (var mapping in autoGenMappings)
            {
                var dbCol = tableSchema.Columns.FirstOrDefault(c => c.Name == mapping.DatabaseColumn);
                var val = GenerateAutoValue(dbCol?.DataType, config.PrimaryKeyGenerationStrategy);
                processedRow[mapping.DatabaseColumn] = val ?? (object)"(database default)";
            }

            foreach (var mapping in regularMappings)
            {
                row.TryGetValue(mapping.ExcelColumn, out var rawValue);

                if (fkMap.TryGetValue(mapping.DatabaseColumn, out var fkInfo))
                {
                    var lookupRule = config.LookupRules.FirstOrDefault(r =>
                        r.ForeignKeyColumn.Equals(mapping.DatabaseColumn, StringComparison.OrdinalIgnoreCase));

                    var useBuildMode = lookupRule?.ProcessingMode == ForeignKeyProcessingMode.BuildFromExcel
                        && lookupRule.ForeignTableMappings is { Count: > 0 };

                    if (useBuildMode)
                    {
                        try
                        {
                            var (resolvedId, wasCreated) = await _fkBuildService.BuildFromExcelAsync(
                                config.ConnectionString,
                                row,
                                fkInfo.ReferencedTable,
                                fkInfo.ReferencedColumn,
                                lookupRule!.ForeignTableMappings,
                                config.PrimaryKeyGenerationStrategy,
                                simulateOnly: true,
                                scope: null,
                                lookupRule);

                            if (resolvedId != null)
                            {
                                processedRow[mapping.DatabaseColumn] = resolvedId;
                                allResolutions.Add(new LookupResolutionDto
                                {
                                    ColumnName = mapping.DatabaseColumn,
                                    OriginalValue = "(built from Excel)",
                                    LookupTable = fkInfo.ReferencedTable,
                                    ResolvedId = resolvedId,
                                    WasCreated = wasCreated,
                                    ProcessingMode = "BuildFromExcel"
                                });
                            }
                            else
                            {
                                processedRow[mapping.DatabaseColumn] = null;
                                hasError = true;
                                allErrors.Add(new ValidationErrorDto
                                {
                                    RowNumber = rowNum,
                                    ColumnName = mapping.ExcelColumn,
                                    Message = $"Could not build foreign record in {fkInfo.ReferencedTable}",
                                    ErrorType = "ForeignKeyNotResolvable",
                                    Value = rawValue
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            processedRow[mapping.DatabaseColumn] = null;
                            hasError = true;
                            allErrors.Add(new ValidationErrorDto
                            {
                                RowNumber = rowNum,
                                ColumnName = mapping.ExcelColumn,
                                Message = $"FK build error: {ex.Message}",
                                ErrorType = "ForeignKeyNotResolvable",
                                Value = rawValue
                            });
                        }
                    }
                    else if (lookupRule?.ProcessingMode == ForeignKeyProcessingMode.UseValueDirectly)
                    {
                        var dbCol = tableSchema.Columns.FirstOrDefault(c => c.Name == mapping.DatabaseColumn);
                        var transformed = _transformationEngine.Transform(rawValue, mapping, dbCol?.DataType);
                        processedRow[mapping.DatabaseColumn] = ResolveValue(transformed, rawValue, dbCol?.DataType);
                    }
                    else if (!string.IsNullOrWhiteSpace(rawValue))
                    {
                        try
                        {
                            var (resolvedId, wasCreated) = await _fkResolutionService.ResolveAsync(
                                config.ConnectionString, fkInfo, lookupRule, rawValue!,
                                config.PrimaryKeyGenerationStrategy);

                            if (resolvedId != null)
                            {
                                processedRow[mapping.DatabaseColumn] = resolvedId;
                                allResolutions.Add(new LookupResolutionDto
                                {
                                    ColumnName = mapping.DatabaseColumn,
                                    OriginalValue = rawValue!,
                                    LookupTable = fkInfo.ReferencedTable,
                                    ResolvedId = resolvedId,
                                    WasCreated = wasCreated,
                                    ProcessingMode = "Lookup"
                                });
                            }
                            else
                            {
                                processedRow[mapping.DatabaseColumn] = rawValue;
                                hasError = true;
                                allErrors.Add(new ValidationErrorDto
                                {
                                    RowNumber = rowNum,
                                    ColumnName = mapping.ExcelColumn,
                                    Message = $"Could not resolve foreign key value '{rawValue}' in {fkInfo.ReferencedTable}",
                                    ErrorType = "ForeignKeyNotResolvable",
                                    Value = rawValue
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            processedRow[mapping.DatabaseColumn] = rawValue;
                            hasError = true;
                            allErrors.Add(new ValidationErrorDto
                            {
                                RowNumber = rowNum,
                                ColumnName = mapping.ExcelColumn,
                                Message = $"FK resolution error: {ex.Message}",
                                ErrorType = "ForeignKeyNotResolvable",
                                Value = rawValue
                            });
                        }
                    }
                    else
                    {
                        processedRow[mapping.DatabaseColumn] = null;
                    }
                }
                else
                {
                    var dbCol = tableSchema.Columns.FirstOrDefault(c => c.Name == mapping.DatabaseColumn);
                    var transformed = _transformationEngine.Transform(rawValue, mapping, dbCol?.DataType);
                    processedRow[mapping.DatabaseColumn] = ResolveValue(transformed, rawValue, dbCol?.DataType);
                }
            }

            foreach (var err in errors)
            {
                allErrors.Add(new ValidationErrorDto
                {
                    RowNumber = err.RowNumber,
                    ColumnName = err.ColumnName,
                    Message = err.Message,
                    ErrorType = err.ErrorType.ToString(),
                    Value = err.Value
                });
            }

            if (previewRows.Count < 100)
                previewRows.Add(processedRow);

            if (hasError) result.ErrorRows++;
            else result.ValidRows++;
        }

        result.Rows = previewRows;
        result.Errors = allErrors;
        result.LookupResolutions = allResolutions;
        return result;
    }

    public async Task<UploadCommitResultDto> CommitAsync(MappingConfigDto config)
    {
        var session = _sessionService.Get(config.FileId)
            ?? throw new InvalidOperationException("Upload session not found. Please upload the file again.");

        var excelData = session.ExcelData
            ?? throw new InvalidOperationException("No Excel data in session.");

        var tableSchema = await _dbProvider.GetTableSchemaAsync(config.ConnectionString, config.TableName);
        var fkMap = tableSchema.ForeignKeys.ToDictionary(fk => fk.ColumnName, StringComparer.OrdinalIgnoreCase);
        var (autoGenMappings, regularMappings) = SplitMappings(config.Mappings);

        var columns = BuildCommitColumns(autoGenMappings, regularMappings, tableSchema, config.PrimaryKeyGenerationStrategy);
        var warnings = new List<string>();

        var commitDuplicateErrors = config.DuplicateKeyColumns is { Count: > 0 } commitDupCols
            ? _validationService.ValidateBatchDuplicates(excelData.Rows, config.Mappings, commitDupCols)
            : new List<ValidationError>();
        var commitDuplicateRowNums = commitDuplicateErrors.Select(e => e.RowNumber).ToHashSet();

        await using var scope = await _dbProvider.BeginTransactionAsync(config.ConnectionString);
        try
        {
            var rows = new List<List<object?>>();
            int lookupCreated = 0;

            for (int i = 0; i < excelData.Rows.Count; i++)
            {
                var rowNum = i + 2;
                if (commitDuplicateRowNums.Contains(rowNum))
                {
                    warnings.Add($"Row {rowNum} skipped: duplicate key in batch");
                    continue;
                }

                var row = excelData.Rows[i];
                var errors = _validationService.ValidateRow(rowNum, row, config.Mappings, tableSchema);

                if (errors.Count > 0)
                {
                    warnings.Add($"Row {rowNum} skipped due to validation errors");
                    continue;
                }

                var rowValues = new List<object?>();
                bool skipRow = false;
                var mappingByCol = config.Mappings.ToDictionary(m => m.DatabaseColumn, StringComparer.OrdinalIgnoreCase);

                foreach (var col in columns)
                {
                    if (!mappingByCol.TryGetValue(col, out var mapping)) continue;

                    if (mapping.AutoGenerate)
                    {
                        var dbCol = tableSchema.Columns.FirstOrDefault(c => c.Name == mapping.DatabaseColumn);
                        var val = GenerateAutoValue(dbCol?.DataType, config.PrimaryKeyGenerationStrategy);
                        rowValues.Add(val);
                        continue;
                    }

                    row.TryGetValue(mapping.ExcelColumn, out var rawValue);

                    if (fkMap.TryGetValue(mapping.DatabaseColumn, out var fkInfo))
                    {
                        var lookupRule = config.LookupRules.FirstOrDefault(r =>
                            r.ForeignKeyColumn.Equals(mapping.DatabaseColumn, StringComparison.OrdinalIgnoreCase));

                        var useBuildMode = lookupRule?.ProcessingMode == ForeignKeyProcessingMode.BuildFromExcel
                            && lookupRule.ForeignTableMappings is { Count: > 0 };

                        if (useBuildMode)
                        {
                            var (resolvedId, wasCreated) = await _fkBuildService.BuildFromExcelAsync(
                                config.ConnectionString,
                                row,
                                fkInfo.ReferencedTable,
                                fkInfo.ReferencedColumn,
                                lookupRule!.ForeignTableMappings,
                                config.PrimaryKeyGenerationStrategy,
                                simulateOnly: false,
                                scope,
                                lookupRule);

                            if (resolvedId == null)
                            {
                                warnings.Add($"Row {i + 2}: Could not build foreign record in {fkInfo.ReferencedTable}");
                                skipRow = true;
                                break;
                            }

                            rowValues.Add(resolvedId);
                            if (wasCreated) lookupCreated++;
                        }
                        else if (lookupRule?.ProcessingMode == ForeignKeyProcessingMode.UseValueDirectly)
                        {
                            var dbCol = tableSchema.Columns.FirstOrDefault(c => c.Name == mapping.DatabaseColumn);
                            var transformed = _transformationEngine.Transform(rawValue, mapping, dbCol?.DataType);
                            rowValues.Add(ResolveValue(transformed, rawValue, dbCol?.DataType));
                        }
                        else if (!string.IsNullOrWhiteSpace(rawValue))
                        {
                            var (resolvedId, wasCreated) = await _fkResolutionService.ResolveAsync(
                                config.ConnectionString, fkInfo, lookupRule, rawValue!,
                                config.PrimaryKeyGenerationStrategy,
                                scope);

                            if (resolvedId == null)
                            {
                                warnings.Add($"Row {i + 2}: Could not resolve FK for '{rawValue}'");
                                skipRow = true;
                                break;
                            }

                            rowValues.Add(resolvedId);
                            if (wasCreated) lookupCreated++;
                        }
                        else
                        {
                            rowValues.Add(null);
                        }
                    }
                    else
                    {
                        var dbCol = tableSchema.Columns.FirstOrDefault(c => c.Name == mapping.DatabaseColumn);
                        var transformed = _transformationEngine.Transform(rawValue, mapping, dbCol?.DataType);
                        rowValues.Add(ResolveValue(transformed, rawValue, dbCol?.DataType));
                    }
                }

                if (!skipRow)
                    rows.Add(rowValues);
            }

            if (rows.Count == 0)
            {
                return new UploadCommitResultDto
                {
                    Success = false,
                    ErrorMessage = "No valid rows to insert",
                    Warnings = warnings
                };
            }

            if (autoGenMappings.Count == 0
                && config.PrimaryKeyGenerationStrategy == PrimaryKeyGenerationStrategy.Uuid
                && tableSchema.PrimaryKeys.Count == 1)
            {
                var pkCol = tableSchema.Columns.FirstOrDefault(c =>
                    tableSchema.PrimaryKeys.Contains(c.Name) && c.DataType?.ToLower() == "uuid");
                if (pkCol != null && !columns.Contains(pkCol.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var pkColumnName = pkCol.Name;
                    columns = new List<string> { pkColumnName }.Concat(columns).ToList();
                    rows = rows.Select(r => new List<object?> { Guid.NewGuid() }.Concat(r).ToList()).ToList();
                }
            }

            await scope.InsertBatchAsync(config.TableName, columns, rows);
            await scope.CommitAsync();

            return new UploadCommitResultDto
            {
                Success = true,
                InsertedCount = rows.Count,
                SkippedCount = excelData.RowCount - rows.Count,
                LookupRecordsCreated = lookupCreated,
                Warnings = warnings
            };
        }
        catch
        {
            throw;
        }
    }

    private static (List<ColumnMapping> AutoGen, List<ColumnMapping> Regular) SplitMappings(List<ColumnMapping> mappings)
    {
        var autoGen = new List<ColumnMapping>();
        var regular = new List<ColumnMapping>();
        foreach (var m in mappings)
        {
            if (m.AutoGenerate) autoGen.Add(m);
            else regular.Add(m);
        }
        return (autoGen, regular);
    }

    private static List<string> BuildCommitColumns(
        List<ColumnMapping> autoGenMappings,
        List<ColumnMapping> regularMappings,
        TableSchema tableSchema,
        PrimaryKeyGenerationStrategy strategy)
    {
        var columns = new List<string>();
        foreach (var m in autoGenMappings)
        {
            var dbCol = tableSchema.Columns.FirstOrDefault(c => c.Name == m.DatabaseColumn);
            var dt = dbCol?.DataType?.ToLower() ?? "";
            if (strategy == PrimaryKeyGenerationStrategy.Uuid
                && (dt == "uuid" || dt == "text" || dt == "character varying" || dt == "varchar"))
            {
                columns.Add(m.DatabaseColumn);
            }
        }
        foreach (var m in regularMappings)
            columns.Add(m.DatabaseColumn);
        return columns;
    }

    private static object? GenerateAutoValue(string? dataType, PrimaryKeyGenerationStrategy strategy)
    {
        if (strategy == PrimaryKeyGenerationStrategy.DatabaseDefault)
            return null;
        var dt = dataType?.ToLower() ?? "";
        if (dt == "uuid")
            return Guid.NewGuid();
        if (dt == "text" || dt == "character varying" || dt == "varchar")
            return Guid.NewGuid().ToString();
        return null;
    }

    private static object? ResolveValue(object? transformed, string? rawValue, string? dataType)
    {
        var value = transformed ?? (object?)rawValue;
        if (value is bool b && dataType?.ToLower() == "boolean")
            return b;
        return ConvertValue(value?.ToString(), dataType);
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
