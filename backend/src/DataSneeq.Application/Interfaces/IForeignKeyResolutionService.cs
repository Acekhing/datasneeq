using DataSneeq.Domain.Enums;
using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Interfaces;

public interface IForeignKeyResolutionService
{
    Task<(object? resolvedId, bool wasCreated)> ResolveAsync(
        string connectionString,
        ForeignKeyInfo fkInfo,
        LookupRule? lookupRule,
        string excelValue,
        PrimaryKeyGenerationStrategy pkStrategy = PrimaryKeyGenerationStrategy.Uuid,
        IDbTransactionScope? scope = null);
}
