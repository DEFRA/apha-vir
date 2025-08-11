using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface IIsolateViabilityRepository
{
    Task<IEnumerable<IsolateViability>> GetViabilityByIsolateIdAsync(Guid isolateId);
}
