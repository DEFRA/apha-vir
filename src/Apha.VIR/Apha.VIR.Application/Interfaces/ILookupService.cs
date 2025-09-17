using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;

namespace Apha.VIR.Application.Interfaces
{
    public interface ILookupService
    {
        Task<IEnumerable<LookupDto>> GetAllLookupsAsync();
        Task<LookupDto> GetLookupByIdAsync(Guid lookupId);
        Task<LookupItemDto> GetLookupItemAsync(Guid lookupId, Guid lookupItemId);
        Task<PaginatedResult<LookupItemDto>> GetAllLookupItemsAsync(Guid lookupId, int pageNo, int pageSize);
        Task<IEnumerable<LookupItemDto>> GetAllLookupItemsAsync(Guid lookupId);
        Task<IEnumerable<LookupItemDto>> GetLookupItemParentListAsync(Guid lookupId);
        Task<bool> IsLookupItemInUseAsync(Guid lookupId, Guid lookupItemId);
        Task InsertLookupItemAsync(Guid LookupId, LookupItemDto Item);
        Task UpdateLookupItemAsync(Guid LookupId, LookupItemDto Item);
        Task DeleteLookupItemAsync(Guid LookupId, LookupItemDto Item);
        Task<IEnumerable<LookupItemDto>> GetAllVirusFamiliesAsync();
        Task<IEnumerable<LookupItemDto>> GetAllVirusTypesAsync();
        Task<IEnumerable<LookupItemDto>> GetAllVirusTypesByParentAsync(Guid? virusFamily);
        Task<IEnumerable<LookupItemDto>> GetAllHostSpeciesAsync();
        Task<IEnumerable<LookupItemDto>> GetAllHostBreedsAsync();
        Task<IEnumerable<LookupItemDto>> GetAllHostBreedsByParentAsync(Guid? hostSpecies);
        Task<IEnumerable<LookupItemDto>> GetAllHostBreedsAltNameAsync();
        Task<IEnumerable<LookupItemDto>> GetAllCountriesAsync();
        Task<IEnumerable<LookupItemDto>> GetAllHostPurposesAsync();
        Task<IEnumerable<LookupItemDto>> GetAllSampleTypesAsync();
        Task<IEnumerable<LookupItemDto>> GetAllWorkGroupsAsync();
        Task<IEnumerable<LookupItemDto>> GetAllStaffAsync();
        Task<IEnumerable<LookupItemDto>> GetAllViabilityAsync();
        Task<IEnumerable<LookupItemDto>> GetAllSubmittingLabAsync();
        Task<IEnumerable<LookupItemDto>> GetAllSubmissionReasonAsync();
        Task<IEnumerable<LookupItemDto>> GetAllIsolationMethodsAsync();
        Task<IEnumerable<LookupItemDto>> GetAllFreezerAsync();
        Task<IEnumerable<LookupItemDto>> GetAllTraysAsync();
        Task<IEnumerable<LookupItemDto>> GetAllTraysByParentAsync(Guid? freezer);
    }
}
