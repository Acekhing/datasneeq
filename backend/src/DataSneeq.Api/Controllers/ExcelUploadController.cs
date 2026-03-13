using DataSneeq.Application.DTOs;
using DataSneeq.Application.Interfaces;
using DataSneeq.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataSneeq.Api.Controllers;

[ApiController]
[Route("api/upload")]
public class ExcelUploadController : ControllerBase
{
    private readonly IExcelParsingService _excelService;
    private readonly IUploadSessionService _sessionService;

    public ExcelUploadController(IExcelParsingService excelService, IUploadSessionService sessionService)
    {
        _excelService = excelService;
        _sessionService = sessionService;
    }

    [HttpPost("excel")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ExcelUploadResultDto>> UploadExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
            return BadRequest("Only .xlsx and .xls files are supported.");

        using var stream = file.OpenReadStream();
        var excelData = await _excelService.ParseExcelFileAsync(stream, file.FileName);

        var session = new UploadSession
        {
            FileId = excelData.FileId,
            ExcelData = excelData
        };
        _sessionService.Store(session);

        return Ok(new ExcelUploadResultDto
        {
            FileId = excelData.FileId,
            Columns = excelData.Headers,
            RowCount = excelData.RowCount,
            SampleRows = excelData.Rows.Take(5).ToList(),
            FileName = excelData.FileName
        });
    }
}
