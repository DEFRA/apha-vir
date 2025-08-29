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

        public async Task<IEnumerable<LookupDTO>> GetAllLookupsAsync()
        {
            return _mapper.Map<IEnumerable<LookupDTO>>(await _lookupRepository.GetAllLookupsAsync());
        }

        public async Task<LookupDTO> GetLookupByIdAsync(Guid lookupId)
        {
            return _mapper.Map<LookupDTO>(await _lookupRepository.GetLookupByIdAsync(lookupId));
        }

        public async Task<LookupItemDTO> GetLookupItemAsync(Guid lookupId, Guid lookupItemId)
        {
            return _mapper.Map<LookupItemDTO>(
                await _lookupRepository.GetLookupItemAsync(lookupId, lookupItemId));
        }

        public async Task<PaginatedResult<LookupItemDTO>> GetAllLookupItemsAsync(Guid lookupId, int pageNo, int pageSize)
        {
            ArgumentNullException.ThrowIfNull(lookupId);
            ArgumentNullException.ThrowIfNull(pageNo);
            ArgumentNullException.ThrowIfNull(pageSize);

            return _mapper.Map<PaginatedResult<LookupItemDTO>>(
                await _lookupRepository.GetAllLookupItemsAsync(lookupId, pageNo, pageSize));
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllLookupItemsAsync(Guid lookupId)
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(
                await _lookupRepository.GetAllLookupItemsAsync(lookupId));
        }

        public async Task<IEnumerable<LookupItemDTO>> GetLookupItemParentListAsync(Guid lookupId)
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(
                await _lookupRepository.GetLookupItemParentListAsync(lookupId));
        }

        public async Task<bool> IsLookupItemInUseAsync(Guid lookupId, Guid lookupItemId)
        {
            return await _lookupRepository.IsLookupItemInUseAsync(lookupId, lookupItemId);
        }

        public async Task InsertLookupItemAsync(Guid LookupId, LookupItemDTO Item)
        {
            var itemData = _mapper.Map<LookupItem>(Item);

            await _lookupRepository.InsertLookupItemAsync(LookupId, itemData);
        }

        public async Task UpdateLookupItemAsync(Guid LookupId, LookupItemDTO Item)
        {
            var itemData = _mapper.Map<LookupItem>(Item);

            await _lookupRepository.UpdateLookupItemAsync(LookupId, itemData);
        }

        public async Task DeleteLookupItemAsync(Guid LookupId, LookupItemDTO Item)
        {
            var itemData = _mapper.Map<LookupItem>(Item);

            await _lookupRepository.DeleteLookupItemAsync(LookupId, itemData);
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllVirusFamiliesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllVirusFamiliesAsync());
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllVirusTypesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllVirusTypesAsync());
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllVirusTypesByParentAsync(Guid? virusFamily)
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllVirusTypesByParentAsync(virusFamily));
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllHostSpeciesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllHostSpeciesAsync());
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllHostBreedsAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllHostBreedsAsync());
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllHostBreedsByParentAsync(Guid? hostSpecies)
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllHostBreedsByParentAsync(hostSpecies));
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllHostBreedsAltNameAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllHostBreedsAltNameAsync());
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllHostPurposesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllHostPurposesAsync());
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllCountriesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllCountriesAsync());
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllSampleTypesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllSampleTypesAsync());
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllWorkGroupsAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllWorkGroupsAsync());
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllStaffAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllStaffAsync());
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllViabilityAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllViabilityAsync());
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllSubmittingLabAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllSubmittingLabAsync());
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllSubmissionReasonAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllSubmissionReasonAsync());
        }
    }
}
