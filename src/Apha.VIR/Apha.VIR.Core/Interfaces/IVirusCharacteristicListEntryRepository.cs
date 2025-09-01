using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Pagination;

namespace Apha.VIR.Core.Interfaces;

public interface IVirusCharacteristicListEntryRepository
{
    Task<IEnumerable<VirusCharacteristicListEntry>> GetVirusCharacteristicListEntryByVirusCharacteristic(Guid virusCharacteristicId);
    Task<PagedData<VirusCharacteristicListEntry>> GetVirusCharacteristicListEntries(Guid virusCharacteristicId, int pageNo, int pageSize);
    Task<VirusCharacteristicListEntry?> GetByIdAsync(Guid id);
    Task AddEntryAsync(VirusCharacteristicListEntry entry);
    Task UpdateEntryAsync(VirusCharacteristicListEntry entry);
    Task DeleteEntryAsync(Guid id, byte[] lastModified);
}
