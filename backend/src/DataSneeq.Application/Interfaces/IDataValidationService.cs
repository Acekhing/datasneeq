using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Interfaces;

public interface IDataValidationService
{
    List<ValidationError> ValidateRow(
        int rowNumber,
        Dictionary<string, string> row,
        List<ColumnMapping> mappings,
        TableSchema tableSchema);

    /// <summary>Detects duplicate keys within the batch. Returns errors for duplicate rows (first occurrence wins).</summary>
    List<ValidationError> ValidateBatchDuplicates(
        IReadOnlyList<Dictionary<string, string>> rows,
        List<ColumnMapping> mappings,
        IReadOnlyList<string> duplicateKeyColumns);
}
