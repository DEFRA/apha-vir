using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface ILookupService
    {
        Task<IEnumerable<LookupDTO>> GetAllLookupsAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllLookupEntriesAsync(Guid LookupId);
        Task InsertLookupEntryAsync(Guid LookupId, LookupItemDTO Item);
        Task UpdateLookupEntryAsync(Guid LookupId, LookupItemDTO Item);
        Task DeleeLookupEntryAsync(Guid LookupId, LookupItemDTO Item);
        Task<IEnumerable<LookupItemDTO>> GetAllVirusFamiliesAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllVirusTypesAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllVirusTypesByParentAsync(Guid? virusFamily);
        Task<IEnumerable<LookupItemDTO>> GetAllHostSpeciesAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllHostBreedsAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllHostBreedsByParentAsync(Guid? hostSpecies);
        Task<IEnumerable<LookupItemDTO>> GetAllHostBreedsAltNameAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllCountriesAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllHostPurposesAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllSampleTypesAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllWorkGroupsAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllStaffAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllViabilityAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllSubmittingLabAsync();
        Task<IEnumerable<LookupItemDTO>> GetAllSubmissionReasonAsync();
    }
}
