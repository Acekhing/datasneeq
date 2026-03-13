using Microsoft.Extensions.Caching.Memory;
using DataSneeq.Application.Interfaces;
using DataSneeq.Domain.Models;

namespace DataSneeq.Application.Services;

public class UploadSessionService : IUploadSessionService
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(30);

    public UploadSessionService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Store(UploadSession session)
    {
        session.LastAccessedAt = DateTime.UtcNow;
        _cache.Set(session.FileId, session, new MemoryCacheEntryOptions
        {
            SlidingExpiration = SessionTimeout
        });
    }

    public UploadSession? Get(string fileId)
    {
        if (_cache.TryGetValue<UploadSession>(fileId, out var session))
        {
            session!.LastAccessedAt = DateTime.UtcNow;
            return session;
        }
        return null;
    }

    public void Remove(string fileId)
    {
        _cache.Remove(fileId);
    }
}
