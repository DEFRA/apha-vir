using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces
{
    public interface ILookupRepository
    {
        Task<IEnumerable<Lookup>> GetAllLookupsAsync();
        Task<IEnumerable<LookupItem>> GetAllLookupEntriesAsync(Guid LookupId);
        Task InsertLookupEntryAsync(Guid LookupId, LookupItem Item);
        Task UpdateLookupEntryAsync(Guid LookupId, LookupItem Item);
        Task DeleteLookupEntryAsync(Guid LookupId, LookupItem Item);
        Task<IEnumerable<LookupItem>> GetAllVirusFamiliesAsync();
        Task<IEnumerable<LookupItem>> GetAllVirusTypesAsync();
        Task<IEnumerable<LookupItem>> GetAllVirusTypesByParentAsync(string? virusFamily);
        Task<IEnumerable<LookupItem>> GetAllHostSpeciesAsync();
        Task<IEnumerable<LookupItem>> GetAllHostBreedsAsync();
        Task<IEnumerable<LookupItem>> GetAllHostBreedsByParentAsync(string? hostSpecies);
        Task<IEnumerable<LookupItem>> GetAllCountriesAsync();
        Task<IEnumerable<LookupItem>> GetAllHostPurposesAsync();
        Task<IEnumerable<LookupItem>> GetAllSampleTypesAsync();
    }
}
