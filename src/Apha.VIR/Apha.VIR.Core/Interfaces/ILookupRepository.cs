using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Pagination;

namespace Apha.VIR.Core.Interfaces
{
    public interface ILookupRepository
    {
        Task<IEnumerable<Lookup>> GetAllLookupsAsync();
        Task<Lookup> GetLookupByIdAsync(Guid lookupId);
        Task<LookupItem> GetLookupItemAsync(Guid lookupId, Guid lookupItemId);
        Task<PagedData<LookupItem>> GetAllLookupItemsAsync(Guid lookupId, int pageNo, int pageSize);
        Task<IEnumerable<LookupItem>> GetAllLookupItemsAsync(Guid lookupId);
        Task<IEnumerable<LookupItem>> GetLookupItemParentListAsync(Guid lookupId);
        Task<bool> IsLookupItemInUseAsync(Guid lookupId, Guid lookupItemId);
        Task InsertLookupItemAsync(Guid LookupId, LookupItem Item);
        Task UpdateLookupItemAsync(Guid LookupId, LookupItem Item);
        Task DeleteLookupItemAsync(Guid LookupId, LookupItem Item);
        Task<IEnumerable<LookupItem>> GetAllVirusFamiliesAsync();
        Task<IEnumerable<LookupItem>> GetAllVirusTypesAsync();
        Task<IEnumerable<LookupItem>> GetAllVirusTypesByParentAsync(Guid? virusFamily);
        Task<IEnumerable<LookupItem>> GetAllHostSpeciesAsync();
        Task<IEnumerable<LookupItem>> GetAllHostBreedsAsync();
        Task<IEnumerable<LookupItem>> GetAllHostBreedsByParentAsync(Guid? hostSpecies);
        Task<IEnumerable<LookupItem>> GetAllCountriesAsync();
        Task<IEnumerable<LookupItem>> GetAllHostPurposesAsync();
        Task<IEnumerable<LookupItem>> GetAllSampleTypesAsync();
        Task<IEnumerable<LookupItem>> GetAllWorkGroupsAsync();
        Task<IEnumerable<LookupItem>> GetAllStaffAsync();
        Task<IEnumerable<LookupItem>> GetAllViabilityAsync();
    }
}
