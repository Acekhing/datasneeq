using DataSneeq.Application.DTOs;
using DataSneeq.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DataSneeq.Api.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly IUploadOrchestrationService _orchestrationService;

    public UploadController(IUploadOrchestrationService orchestrationService)
    {
        _orchestrationService = orchestrationService;
    }

    [HttpPost("preview")]
    public async Task<ActionResult<UploadPreviewDto>> Preview([FromBody] MappingConfigDto config)
    {
        var result = await _orchestrationService.PreviewAsync(config);
        return Ok(result);
    }

    [HttpPost("commit")]
    public async Task<ActionResult<UploadCommitResultDto>> Commit([FromBody] MappingConfigDto config)
    {
        var result = await _orchestrationService.CommitAsync(config);
        return Ok(result);
    }
}
