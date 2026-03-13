namespace DataSneeq.Application.Interfaces;

public interface IDbTransactionScope : IAsyncDisposable
{
    Task<object?> LookupValueAsync(string table, string matchColumn, string matchValue, string returnColumn);
    Task<object?> LookupByCompositeAsync(string table, IReadOnlyList<(string Column, object? Value)> matchPairs, string returnColumn);
    Task<object> CreateLookupRecordAsync(string table, string displayColumn, object displayValue, string pkColumn, object? pkValue);
    Task<object> InsertSingleRecordAsync(string table, List<string> columns, List<object?> values, string returnColumn);
    Task InsertBatchAsync(string table, List<string> columns, List<List<object?>> rows);
    Task CommitAsync();
}
