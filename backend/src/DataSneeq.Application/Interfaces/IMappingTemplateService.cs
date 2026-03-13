using DataSneeq.Application.DTOs;

namespace DataSneeq.Application.Interfaces;

public interface IMappingTemplateService
{
    Task<MappingTemplateDto> SaveAsync(SaveMappingTemplateDto dto);
    Task<List<MappingTemplateDto>> GetAllAsync();
    Task<MappingTemplateDto?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}
