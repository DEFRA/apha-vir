using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces;

public interface IIsolateViabilityService
{
    Task<IEnumerable<IsolateViabilityInfoDTO>> GetViabilityByIsolateIdAsync(Guid isolateId);
    Task<IEnumerable<IsolateViabilityInfoDTO>> GetViabilityHistoryAsync(string AVNumber, Guid IsolateId);
    Task DeleteIsolateViabilityAsync(Guid IsolateId, byte[] lastModified, string userid);
    Task UpdateIsolateViabilityAsync(IsolateViabilityInfoDTO isolateViability, string userid);
    Task AddIsolateViabilityAsync(IsolateViabilityInfoDTO isolateViability, string userId);
    Task<IsolateViabilityDTO?> GetLastViabilityByIsolateAsync(Guid IsolateId);
}
