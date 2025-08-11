using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces;

public interface IIsolateViabilityService
{
    Task<IEnumerable<IsolateViabilityInfoDTO>> GetViabilityHistoryAsync(string AVNumber, Guid IsolateId);
    Task DeleteIsolateViabilityAsync(Guid IsolateId, byte[] lastModified, string userid);
    Task UpdateIsolateViabilityAsync(IsolateViabilityInfoDTO isolateViability, string userid);
}
