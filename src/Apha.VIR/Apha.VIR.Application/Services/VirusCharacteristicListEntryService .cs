using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class VirusCharacteristicListEntryService : IVirusCharacteristicListEntryService
    {
        private readonly IVirusCharacteristicListEntryRepository _repository;
        private readonly IMapper _mapper;

        public VirusCharacteristicListEntryService(IVirusCharacteristicListEntryRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper;
        }

        public async Task<IEnumerable<VirusCharacteristicListEntryDTO>> GetEntriesByCharacteristicIdAsync(Guid virusCharacteristicId)
        {
            var entities = await _repository.GetEntriesByCharacteristicIdAsync(virusCharacteristicId);
            return _mapper.Map<IEnumerable<VirusCharacteristicListEntryDTO>>(entities);
        }

        public async Task<PaginatedResult<VirusCharacteristicListEntryDTO>> GetVirusCharacteristicListEntries(Guid virusCharacteristicId, int pageNo, int pageSize)
        {
            var entities = await _repository.GetVirusCharacteristicListEntries(virusCharacteristicId, pageNo, pageSize);
            return _mapper.Map<PaginatedResult<VirusCharacteristicListEntryDTO>>(entities);
        }

        public async Task<VirusCharacteristicListEntryDTO?> GetEntryByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<VirusCharacteristicListEntryDTO>(entity);
        }

        public async Task AddEntryAsync(VirusCharacteristicListEntryDTO dto)
        {
            dto.Id = Guid.NewGuid();
            await _repository.AddEntryAsync(_mapper.Map<VirusCharacteristicListEntry>(dto));
        }

        public async Task UpdateEntryAsync(VirusCharacteristicListEntryDTO dto)
        {
            await _repository.UpdateEntryAsync(_mapper.Map<VirusCharacteristicListEntry>(dto));
        }

        public async Task DeleteEntryAsync(Guid id, byte[] lastModified)
        {
            await _repository.DeleteEntryAsync(id, lastModified);
        }
    }
}
