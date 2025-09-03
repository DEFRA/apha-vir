using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface IIsolateViabilityRepository
{
    Task<IEnumerable<IsolateViability>> GetViabilityHistoryAsync(Guid IsolateId);
    Task DeleteIsolateViabilityAsync(Guid IsolateId, byte[] lastModified, string userid);
    Task<IEnumerable<IsolateViability>> GetViabilityByIsolateIdAsync(Guid isolateId);
    Task UpdateIsolateViabilityAsync(IsolateViability isolateViability, string userid);
    Task AddIsolateViabilityAsync(IsolateViability isolateViability, string userId);
}