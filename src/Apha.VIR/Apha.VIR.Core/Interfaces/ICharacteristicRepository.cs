using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface ICharacteristicRepository
{
    Task<IEnumerable<IsolateCharacteristicInfo>> GetIsolateCharacteristicInfoAsync(Guid isolateId);
}
