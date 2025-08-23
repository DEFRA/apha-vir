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

        public async Task<LookupDTO> GetLookupsByIdAsync(Guid LookupId)
        {
            return _mapper.Map<LookupDTO>(await _lookupRepository.GetLookupsByIdAsync(LookupId));
        }

        public async Task<IEnumerable<LookupItemDTO>> GetLookupItemParentListAsync(Guid lookupId)
        {
            ArgumentNullException.ThrowIfNull(lookupId);

            return _mapper.Map<IEnumerable<LookupItemDTO>>(
                await _lookupRepository.GetLookupItemParentListAsync(lookupId));
        }
        public async Task<LookupItemDTO> GetLookupItemAsync(Guid lookupId, Guid lookupItemId)
        {
            ArgumentNullException.ThrowIfNull(lookupItemId);

            return _mapper.Map<LookupItemDTO>(
                await _lookupRepository.GetLookupItemAsync(lookupId,lookupItemId));
        }

        public async Task<PaginatedResult<LookupItemDTO>> GetAllLookupEntriesAsync(Guid LookupId,int pageNo,int pageSize)
        {
            ArgumentNullException.ThrowIfNull(LookupId);
            ArgumentNullException.ThrowIfNull(pageNo);
            ArgumentNullException.ThrowIfNull(pageSize);

            return _mapper.Map<PaginatedResult<LookupItemDTO>>(
                await _lookupRepository.GetAllLookupEntriesAsync(LookupId,  pageNo,  pageSize));
        }

        public async Task InsertLookupEntryAsync(Guid LookupId, LookupItemDTO Item)
        {
            var itemData = _mapper.Map<LookupItem>(Item);
            await _lookupRepository.InsertLookupEntryAsync(LookupId, itemData);
        }

        public async Task UpdateLookupEntryAsync(Guid LookupId, LookupItemDTO Item)
        {
            var itemData = _mapper.Map<LookupItem>(Item);
            await _lookupRepository.UpdateLookupEntryAsync(LookupId, itemData);
        }

        public async Task DeleeLookupEntryAsync(Guid LookupId, LookupItemDTO Item)
        {
            var itemData = _mapper.Map<LookupItem>(Item);
            await _lookupRepository.DeleteLookupEntryAsync(LookupId, itemData);
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

        public async Task<IEnumerable<LookupItemDTO>> GetAllCountriesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllCountriesAsync());
        }

        public async Task<IEnumerable<LookupItemDTO>> GetAllHostPurposesAsync()
        {
            return _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllHostPurposesAsync());
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
    }
}
