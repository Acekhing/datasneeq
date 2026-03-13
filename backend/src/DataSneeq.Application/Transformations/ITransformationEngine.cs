using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Transformations;

public interface ITransformationEngine
{
    /// <summary>
    /// Transforms a raw Excel value according to the mapping's transformation rule.
    /// Returns the transformed value, or the original if no transformation applies.
    /// </summary>
    object? Transform(string? rawValue, ColumnMapping mapping, string? targetDataType);
}
