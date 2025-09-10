using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Pagination;

namespace Apha.VIR.Core.Interfaces
{
    public interface IVirusCharacteristicRepository
    {
        Task AddEntryAsync(VirusCharacteristic virusCharacteristic);
        Task<IEnumerable<VirusCharacteristic>> GetAllVirusCharacteristicsAsync();
        Task<PagedData<VirusCharacteristic>> GetAllVirusCharacteristicsAsync(int pageNo = 0, int pageSize = 0);
        Task<IEnumerable<VirusCharacteristic>> GetAllVirusCharacteristicsByVirusTypeAsync(Guid? virusType, bool isAbscent);
        Task<IEnumerable<VirusCharacteristicDataType>> GetAllVirusCharactersticsTypeNamesAsync();

    }
}
