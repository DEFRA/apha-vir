using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface IIsolateRepository
{
    Task<IEnumerable<IsolateInfo>> GetIsolateInfoByAVNumberAsync(string AVNumber);
    Task<IsolateFullDetail> GetIsolateFullDetailsByIdAsync(Guid isolateId);
    Task<Isolate> GetIsolateByIsolateAndAVNumberAsync(string avNumber, Guid isolateId);
    Task<Guid> AddIsolateDetailsAsync(Isolate isolate);
    Task UpdateIsolateDetailsAsync(Isolate isolate);
    Task DeleteIsolateAsync(Guid isolateId, string userId, byte[] lastModified);
    Task<IEnumerable<IsolateNomenclature>> GetIsolateForNomenclatureAsync(Guid isolateId);
}
