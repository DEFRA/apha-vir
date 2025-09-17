using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class LookupService : ILookupService
    {
        private readonly ILookupRepository _lookupRepository;
        private readonly IMapper _mapper;

        public LookupService(ILookupRepository lookupRepository, IMapper mapper)
        {
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
            _mapper = mapper;
        }

        public async Task<IEnumerable<LookupDto>> GetAllLookupsAsync()
        {
            return _mapper.Map<IEnumerable<LookupDto>>(await _lookupRepository.GetAllLookupsAsync());
        }

        public async Task<LookupDto> GetLookupByIdAsync(Guid lookupId)
        {
            return _mapper.Map<LookupDto>(await _lookupRepository.GetLookupByIdAsync(lookupId));
        }

        public async Task<LookupItemDto> GetLookupItemAsync(Guid lookupId, Guid lookupItemId)
        {
            return _mapper.Map<LookupItemDto>(
                await _lookupRepository.GetLookupItemAsync(lookupId, lookupItemId));
        }

        public async Task<PaginatedResult<LookupItemDto>> GetAllLookupItemsAsync(Guid lookupId, int pageNo, int pageSize)
        {
            return _mapper.Map<PaginatedResult<LookupItemDto>>(
                await _lookupRepository.GetAllLookupItemsAsync(lookupId, pageNo, pageSize));
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllLookupItemsAsync(Guid lookupId)
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(
                await _lookupRepository.GetAllLookupItemsAsync(lookupId));
        }

        public async Task<IEnumerable<LookupItemDto>> GetLookupItemParentListAsync(Guid lookupId)
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(
                await _lookupRepository.GetLookupItemParentListAsync(lookupId));
        }

        public async Task<bool> IsLookupItemInUseAsync(Guid lookupId, Guid lookupItemId)
        {
            return await _lookupRepository.IsLookupItemInUseAsync(lookupId, lookupItemId);
        }

        public async Task InsertLookupItemAsync(Guid LookupId, LookupItemDto Item)
        {
            var itemData = _mapper.Map<LookupItem>(Item);

            await _lookupRepository.InsertLookupItemAsync(LookupId, itemData);
        }

        public async Task UpdateLookupItemAsync(Guid LookupId, LookupItemDto Item)
        {
            var itemData = _mapper.Map<LookupItem>(Item);

            await _lookupRepository.UpdateLookupItemAsync(LookupId, itemData);
        }

        public async Task DeleteLookupItemAsync(Guid LookupId, LookupItemDto Item)
        {
            var itemData = _mapper.Map<LookupItem>(Item);

            await _lookupRepository.DeleteLookupItemAsync(LookupId, itemData);
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllVirusFamiliesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllVirusFamiliesAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllVirusTypesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllVirusTypesAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllVirusTypesByParentAsync(Guid? virusFamily)
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllVirusTypesByParentAsync(virusFamily));
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllHostSpeciesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllHostSpeciesAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllHostBreedsAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllHostBreedsAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllHostBreedsByParentAsync(Guid? hostSpecies)
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllHostBreedsByParentAsync(hostSpecies));
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllHostBreedsAltNameAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllHostBreedsAltNameAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllHostPurposesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllHostPurposesAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllCountriesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllCountriesAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllSampleTypesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllSampleTypesAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllWorkGroupsAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllWorkGroupsAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllStaffAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllStaffAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllViabilityAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllViabilityAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllSubmittingLabAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllSubmittingLabAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllSubmissionReasonAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllSubmissionReasonAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllIsolationMethodsAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllIsolationMethodsAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllFreezerAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllFreezerAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllTraysAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllTraysAsync());
        }

        public async Task<IEnumerable<LookupItemDto>> GetAllTraysByParentAsync(Guid? freezer)
        {
            return _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllTraysByParentAsync(freezer));
        }
    }
}
