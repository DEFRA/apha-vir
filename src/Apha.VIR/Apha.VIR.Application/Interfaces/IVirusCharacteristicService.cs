using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;

namespace Apha.VIR.Application.Interfaces
{
    public interface IVirusCharacteristicService
    {
        Task AddEntryAsync(VirusCharacteristicDTO dto);
        Task<IEnumerable<VirusCharacteristicDTO>> GetAllVirusCharacteristicsAsync();
        Task<PaginatedResult<VirusCharacteristicDTO>> GetAllVirusCharacteristicsAsync(int pageNo = 0, int pageSize = 0);
        Task<IEnumerable<VirusCharacteristicDTO>> GetAllVirusCharacteristicsByVirusTypeAsync(Guid? virusType, bool isAbscent);
        Task<IEnumerable<VirusCharacteristicDataTypeDTO>> GetAllVirusCharactersticsTypeNamesAsync();

    }
}