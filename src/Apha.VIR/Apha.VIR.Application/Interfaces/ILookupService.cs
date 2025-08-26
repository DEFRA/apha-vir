using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;

namespace Apha.VIR.Application.Interfaces
{
    public interface ILookupService
    {
        Task<IEnumerable<LookupDTO>> GetAllLookupsAsync();
        Task<LookupDTO> GetLookupsByIdAsync(Guid lookupId);
        Task<LookupItemDTO> GetLookupItemAsync(Guid lookupId, Guid lookupItemId);
        Task<PaginatedResult<LookupItemDTO>> GetAllLookupItemsAsync(Guid lookupId, int pageNo, int pageSize);
        Task<IEnumerable<LookupItemDTO>> GetAllLookupItemsAsync(Guid lookupId);
        Task<IEnumerable<LookupItemDTO>> GetLookupItemParentListAsync(Guid lookupId);
        Task<bool> IsLookupItemInUseAsync(Guid lookupId, Guid lookupItemId);
        Task InsertLookupItemAsync(Guid LookupId, LookupItemDTO Item);
        Task UpdateLookupItemAsync(Guid LookupId, LookupItemDTO Item);
        Task DeleteLookupItemAsync(Guid LookupId, LookupItemDTO Item);
        Task<IEnumerable<LookupItemDTO>> GetAllVirusFamiliesAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllVirusTypesAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllVirusTypesByParentAsync(Guid? virusFamily);
        Task<IEnumerable<LookupItemDTO>> GetAllHostSpeciesAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllHostBreedsAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllHostBreedsByParentAsync(Guid? hostSpecies);
        Task<IEnumerable<LookupItemDTO>> GetAllCountriesAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllHostPurposesAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllSampleTypesAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllWorkGroupsAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllStaffAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllViabilityAsync();
    }
}
