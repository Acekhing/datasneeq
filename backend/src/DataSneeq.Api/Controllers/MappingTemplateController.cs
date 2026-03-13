using DataSneeq.Application.DTOs;
using DataSneeq.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DataSneeq.Api.Controllers;

[ApiController]
[Route("api/mapping-templates")]
public class MappingTemplateController : ControllerBase
{
    private readonly IMappingTemplateService _templateService;

    public MappingTemplateController(IMappingTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpPost]
    public async Task<ActionResult<MappingTemplateDto>> Save([FromBody] SaveMappingTemplateDto dto)
    {
        var result = await _templateService.SaveAsync(dto);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<MappingTemplateDto>>> GetAll()
    {
        var templates = await _templateService.GetAllAsync();
        return Ok(templates);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MappingTemplateDto>> GetById(Guid id)
    {
        var template = await _templateService.GetByIdAsync(id);
        if (template == null) return NotFound();
        return Ok(template);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _templateService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
