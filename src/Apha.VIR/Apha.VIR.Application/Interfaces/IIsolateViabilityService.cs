using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces;

public interface IIsolateViabilityService
{
    Task<IEnumerable<IsolateViabilityInfoDto>> GetViabilityByIsolateIdAsync(Guid isolateId);
    Task<IEnumerable<IsolateViabilityInfoDto>> GetViabilityHistoryAsync(string AVNumber, Guid IsolateId);
    Task DeleteIsolateViabilityAsync(Guid IsolateId, byte[] lastModified, string userid);
    Task UpdateIsolateViabilityAsync(IsolateViabilityInfoDto isolateViability, string userid);
    Task AddIsolateViabilityAsync(IsolateViabilityInfoDto isolateViability, string userId);
    Task<IsolateViabilityDto?> GetLastViabilityByIsolateAsync(Guid IsolateId);
}
