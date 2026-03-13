using DataSneeq.Application.DTOs;

namespace DataSneeq.Application.Interfaces;

public interface IUploadOrchestrationService
{
    Task<UploadPreviewDto> PreviewAsync(MappingConfigDto config);
    Task<UploadCommitResultDto> CommitAsync(MappingConfigDto config);
}
