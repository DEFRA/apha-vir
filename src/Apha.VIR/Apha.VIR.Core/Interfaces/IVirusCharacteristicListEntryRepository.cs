using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface IVirusCharacteristicListEntryRepository
{
    Task<IEnumerable<VirusCharacteristicListEntry>> GetVirusCharacteristicListEntryByVirusCharacteristic(Guid virusCharacteristicId);
}
