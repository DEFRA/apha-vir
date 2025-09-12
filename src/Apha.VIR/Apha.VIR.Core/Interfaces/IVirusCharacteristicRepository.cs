using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Pagination;

namespace Apha.VIR.Core.Interfaces
{
    public interface IVirusCharacteristicRepository
    {
        Task AddEntryAsync(VirusCharacteristic virusCharacteristic);
        Task<bool> CheckVirusCharactersticsUsageByIdAsync(Guid id);
        Task DeleteVirusCharactersticsAsync(Guid id, byte[] lastModified);
        Task<IEnumerable<VirusCharacteristic>> GetAllVirusCharacteristicsAsync();
        Task<PagedData<VirusCharacteristic>> GetAllVirusCharacteristicsAsync(int pageNo, int pageSize);
        Task<IEnumerable<VirusCharacteristic>> GetAllVirusCharacteristicsByVirusTypeAsync(Guid? virusType, bool isAbscent);
        Task<IEnumerable<VirusCharacteristicDataType>> GetAllVirusCharactersticsTypeNamesAsync();
        Task<VirusCharacteristic> GetVirusCharacteristicsByIdAsync(Guid? id);
        Task UpdateEntryAsync(VirusCharacteristic virusCharacteristic);
    }
}
