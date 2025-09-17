using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;

namespace Apha.VIR.Application.Interfaces
{
    public interface IVirusCharacteristicService
    {
        Task AddEntryAsync(VirusCharacteristicDto dto);
        Task<IEnumerable<VirusCharacteristicDto>> GetAllVirusCharacteristicsAsync();
        Task<PaginatedResult<VirusCharacteristicDto>> GetAllVirusCharacteristicsAsync(int pageNo, int pageSize);
        Task<VirusCharacteristicDto?> GetVirusCharacteristicsByIdAsync(Guid id);
        Task<IEnumerable<VirusCharacteristicDto>> GetAllVirusCharacteristicsByVirusTypeAsync(Guid? virusType, bool isAbscent);
        Task<IEnumerable<VirusCharacteristicDataTypeDto>> GetAllVirusCharactersticsTypeNamesAsync();
        Task UpdateEntryAsync(VirusCharacteristicDto dto);
        Task DeleteVirusCharactersticsAsync(Guid id, byte[] lastModified);
        Task<bool> CheckVirusCharactersticsUsageByIdAsync(Guid id);
    }
}