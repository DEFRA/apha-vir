using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Pagination;

namespace Apha.VIR.Core.Interfaces
{
    public interface ILookupRepository
    {
        Task<IEnumerable<Lookup>> GetAllLookupsAsync();
        Task<Lookup> GetLookupsByIdAsync(Guid lookupId);
        Task<PagedData<LookupItem>> GetAllLookupEntriesAsync(Guid lookupId, int pageNo, int pageSize);
        Task InsertLookupEntryAsync(Guid LookupId, LookupItem Item);
        Task UpdateLookupEntryAsync(Guid LookupId, LookupItem Item);
        Task DeleteLookupEntryAsync(Guid LookupId, LookupItem Item);
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
