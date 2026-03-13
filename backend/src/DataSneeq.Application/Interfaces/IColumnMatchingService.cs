using DataSneeq.Application.DTOs;
using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Interfaces;

public interface IColumnMatchingService
{
    List<MappingSuggestionDto> SuggestMappings(List<string> excelColumns, List<ColumnSchema> dbColumns);
}
