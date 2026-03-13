using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Interfaces;

public interface IUploadSessionService
{
    void Store(UploadSession session);
    UploadSession? Get(string fileId);
    void Remove(string fileId);
}
