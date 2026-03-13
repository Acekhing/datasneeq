using DataSneeq.Domain.Enums;
using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Interfaces;

public interface IForeignKeyBuildService
{
    /// <summary>
    /// Creates a record in the referenced table from Excel row data using foreign table mappings,
    /// then returns the generated primary key. When simulateOnly is true, validates but does not insert.
    /// </summary>
    Task<(object? resolvedId, bool wasCreated)> BuildFromExcelAsync(
        string connectionString,
        IReadOnlyDictionary<string, string> excelRow,
        string referencedTable,
        string returnColumn,
        List<ColumnMapping> foreignTableMappings,
        PrimaryKeyGenerationStrategy pkStrategy = PrimaryKeyGenerationStrategy.Uuid,
        bool simulateOnly = false,
        IDbTransactionScope? scope = null,
        LookupRule? lookupRule = null);
}
