using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;

namespace Apha.VIR.Application.Interfaces
{
    public interface IVirusCharacteristicListEntryService
    {
        Task<IEnumerable<VirusCharacteristicListEntryDto>> GetEntriesByCharacteristicIdAsync(Guid virusCharacteristicId);
        Task<PaginatedResult<VirusCharacteristicListEntryDto>> GetVirusCharacteristicListEntries(Guid virusCharacteristicId, int pageNo, int pageSize);
        Task<VirusCharacteristicListEntryDto?> GetEntryByIdAsync(Guid id);
        Task AddEntryAsync(VirusCharacteristicListEntryDto dto);
        Task UpdateEntryAsync(VirusCharacteristicListEntryDto dto);
        Task DeleteEntryAsync(Guid id, byte[] lastModified);
    }
}
