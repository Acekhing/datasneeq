using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Interfaces;

public interface IExcelParsingService
{
    Task<ExcelFileData> ParseExcelFileAsync(Stream fileStream, string fileName);
}
