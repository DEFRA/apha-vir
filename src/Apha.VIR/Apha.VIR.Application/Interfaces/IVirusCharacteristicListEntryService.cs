using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;

namespace Apha.VIR.Application.Interfaces
{
    public interface IVirusCharacteristicListEntryService
    {
        Task<IEnumerable<VirusCharacteristicListEntryDTO>> GetEntriesByCharacteristicIdAsync(Guid virusCharacteristicId);
        Task<PaginatedResult<VirusCharacteristicListEntryDTO>> GetVirusCharacteristicListEntries(Guid virusCharacteristicId, int pageNo, int pageSize);
        Task<VirusCharacteristicListEntryDTO?> GetEntryByIdAsync(Guid id);
        Task AddEntryAsync(VirusCharacteristicListEntryDTO dto);
        Task UpdateEntryAsync(VirusCharacteristicListEntryDTO dto);
        Task DeleteEntryAsync(Guid id, byte[] lastModified);
    }
}
