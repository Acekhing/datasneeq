using DataSneeq.Application.Interfaces;
using DataSneeq.Domain.Enums;
using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Services;

public class ForeignKeyResolutionService : IForeignKeyResolutionService
{
    private readonly IDatabaseProvider _dbProvider;

    public ForeignKeyResolutionService(IDatabaseProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }

    public async Task<(object? resolvedId, bool wasCreated)> ResolveAsync(
        string connectionString,
        ForeignKeyInfo fkInfo,
        LookupRule? lookupRule,
        string excelValue,
        PrimaryKeyGenerationStrategy pkStrategy = PrimaryKeyGenerationStrategy.Uuid,
        IDbTransactionScope? scope = null)
    {
        if (string.IsNullOrWhiteSpace(excelValue))
            return (null, false);

        var lookupTable = lookupRule?.LookupTable ?? fkInfo.ReferencedTable;
        var displayColumn = lookupRule?.LookupDisplayColumn ?? fkInfo.LookupDisplayColumn;
        var returnColumn = fkInfo.ReferencedColumn;

        if (string.IsNullOrEmpty(displayColumn))
            return (null, false);

        object? existingId;
        if (scope != null)
        {
            existingId = await scope.LookupValueAsync(lookupTable, displayColumn, excelValue, returnColumn);
        }
        else
        {
            existingId = await _dbProvider.LookupValueAsync(
                connectionString, lookupTable, displayColumn, excelValue, returnColumn);
        }

        if (existingId != null && existingId != DBNull.Value)
            return (existingId, false);

        bool autoCreate = lookupRule?.AutoCreate ?? true;
        if (!autoCreate)
            return (null, false);

        object? pkValue = pkStrategy == PrimaryKeyGenerationStrategy.Uuid ? Guid.NewGuid() : null;
        object newId;
        if (scope != null)
        {
            newId = await scope.CreateLookupRecordAsync(lookupTable, displayColumn, excelValue, returnColumn, pkValue);
        }
        else
        {
            newId = await _dbProvider.CreateLookupRecordAsync(
                connectionString, lookupTable, displayColumn, excelValue, returnColumn, pkValue);
        }

        return (newId, true);
    }
}
