using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;

namespace Apha.VIR.Application.Interfaces
{
    public interface IVirusCharacteristicService
    {
        Task AddEntryAsync(VirusCharacteristicDTO dto);
        Task<IEnumerable<VirusCharacteristicDTO>> GetAllVirusCharacteristicsAsync();
        Task<PaginatedResult<VirusCharacteristicDTO>> GetAllVirusCharacteristicsAsync(int pageNo, int pageSize);
        Task<VirusCharacteristicDTO> GetVirusCharacteristicsByIdAsync(Guid? id);
        Task<IEnumerable<VirusCharacteristicDTO>> GetAllVirusCharacteristicsByVirusTypeAsync(Guid? virusType, bool isAbscent);
        Task<IEnumerable<VirusCharacteristicDataTypeDTO>> GetAllVirusCharactersticsTypeNamesAsync();
        Task UpdateEntryAsync(VirusCharacteristicDTO dto);
        Task DeleteVirusCharactersticsAsync(Guid id, byte[] lastModified);
        Task<bool> CheckVirusCharactersticsUsageByIdAsync(Guid id);
    }
}