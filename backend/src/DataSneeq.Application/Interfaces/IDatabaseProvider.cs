using DataSneeq.Domain.Enums;
using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Interfaces;

public interface IDatabaseProvider
{
    DatabaseProviderType ProviderType { get; }
    Task TestConnectionAsync(string connectionString);
    Task<List<string>> GetTableNamesAsync(string connectionString);
    Task<TableSchema> GetTableSchemaAsync(string connectionString, string tableName);
    Task<object?> LookupValueAsync(string connectionString, string table, string matchColumn, string matchValue, string returnColumn);
    Task<object?> LookupByCompositeAsync(string connectionString, string table, IReadOnlyList<(string Column, object? Value)> matchPairs, string returnColumn);
    Task<object> CreateLookupRecordAsync(string connectionString, string table, string displayColumn, object displayValue, string pkColumn, object? pkValue = null);
    Task<object> InsertSingleRecordAsync(string connectionString, string table, List<string> columns, List<object?> values, string returnColumn);
    Task<BatchInsertResult> BatchInsertAsync(string connectionString, string table, List<string> columns, List<List<object?>> rows, int batchSize = 1000);
    Task<IDbTransactionScope> BeginTransactionAsync(string connectionString);
}
