using DataSneeq.Application.DTOs;
using DataSneeq.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DataSneeq.Api.Controllers;

[ApiController]
[Route("api/mapping")]
public class MappingController : ControllerBase
{
    private readonly IColumnMatchingService _matchingService;
    private readonly IDatabaseProvider _dbProvider;
    private readonly IUploadSessionService _sessionService;

    public MappingController(
        IColumnMatchingService matchingService,
        IDatabaseProvider dbProvider,
        IUploadSessionService sessionService)
    {
        _matchingService = matchingService;
        _dbProvider = dbProvider;
        _sessionService = sessionService;
    }

    [HttpPost("suggest")]
    public async Task<ActionResult<List<MappingSuggestionDto>>> SuggestMappings([FromBody] MappingSuggestRequestDto request)
    {
        var session = _sessionService.Get(request.FileId);
        if (session?.ExcelData == null)
            return NotFound("Upload session not found.");

        var schema = await _dbProvider.GetTableSchemaAsync(request.ConnectionString, request.TableName);
        var suggestions = _matchingService.SuggestMappings(session.ExcelData.Headers, schema.Columns);

        return Ok(suggestions);
    }
}
