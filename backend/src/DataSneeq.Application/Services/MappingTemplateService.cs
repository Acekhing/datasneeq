using System.Text.Json;
using DataSneeq.Application.DTOs;
using DataSneeq.Application.Interfaces;
using DataSneeq.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DataSneeq.Application.Services;

public class MappingTemplateService : IMappingTemplateService
{
    private readonly DbContext _dbContext;

    public MappingTemplateService(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MappingTemplateDto> SaveAsync(SaveMappingTemplateDto dto)
    {
        var entity = new MappingTemplate
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            TargetTable = dto.TargetTable,
            MappingsJson = JsonSerializer.Serialize(dto.Mappings),
            LookupRulesJson = JsonSerializer.Serialize(dto.LookupRules),
            DuplicateKeyColumnsJson = JsonSerializer.Serialize(dto.DuplicateKeyColumns ?? new List<string>()),
            PrimaryKeyGenerationStrategy = dto.PrimaryKeyGenerationStrategy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Set<MappingTemplate>().Add(entity);
        await _dbContext.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<List<MappingTemplateDto>> GetAllAsync()
    {
        var entities = await _dbContext.Set<MappingTemplate>()
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();
        return entities.Select(ToDto).ToList();
    }

    public async Task<MappingTemplateDto?> GetByIdAsync(Guid id)
    {
        var entity = await _dbContext.Set<MappingTemplate>().FindAsync(id);
        return entity == null ? null : ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _dbContext.Set<MappingTemplate>().FindAsync(id);
        if (entity == null) return false;

        _dbContext.Set<MappingTemplate>().Remove(entity);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static MappingTemplateDto ToDto(MappingTemplate entity)
    {
        return new MappingTemplateDto
        {
            Id = entity.Id,
            Name = entity.Name,
            TargetTable = entity.TargetTable,
            Mappings = JsonSerializer.Deserialize<List<ColumnMapping>>(entity.MappingsJson) ?? new(),
            LookupRules = JsonSerializer.Deserialize<List<LookupRule>>(entity.LookupRulesJson) ?? new(),
            DuplicateKeyColumns = JsonSerializer.Deserialize<List<string>>(entity.DuplicateKeyColumnsJson) ?? new(),
            PrimaryKeyGenerationStrategy = entity.PrimaryKeyGenerationStrategy,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
