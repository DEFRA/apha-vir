using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface IVirusCharacteristicListEntryRepository
{
    Task<IEnumerable<VirusCharacteristicListEntry>> GetVirusCharacteristicListEntryByVirusCharacteristic(Guid virusCharacteristicId);
    Task<VirusCharacteristicListEntry?> GetByIdAsync(Guid id);
    Task AddEntryAsync(VirusCharacteristicListEntry entry);
    Task UpdateEntryAsync(VirusCharacteristicListEntry entry);
    Task DeleteEntryAsync(Guid id, byte[] lastModified);
}
