using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class VirusCharacteristicService : IVirusCharacteristicService
    {
        private readonly IVirusCharacteristicRepository _virusCharacteristicRepository;
        private readonly IMapper _mapper;

        public VirusCharacteristicService(IVirusCharacteristicRepository virusCharacteristicRepository, IMapper mapper)
        {
            _virusCharacteristicRepository = virusCharacteristicRepository ?? throw new ArgumentNullException(nameof(virusCharacteristicRepository));
            _mapper = mapper;
        }

        public async Task<IEnumerable<VirusCharacteristicDto>> GetAllVirusCharacteristicsAsync()
        {
            var result = await _virusCharacteristicRepository.GetAllVirusCharacteristicsAsync();
            return _mapper.Map<IEnumerable<VirusCharacteristicDto>>(result);
        }

        public async Task<PaginatedResult<VirusCharacteristicDto>> GetAllVirusCharacteristicsAsync(int pageNo, int pageSize)
        {
            var result = await _virusCharacteristicRepository.GetAllVirusCharacteristicsAsync(pageNo, pageSize);
            return _mapper.Map<PaginatedResult<VirusCharacteristicDto>>(result);
        }

        public async Task<VirusCharacteristicDto?> GetVirusCharacteristicsByIdAsync(Guid id)
        {
            var result = await _virusCharacteristicRepository.GetVirusCharacteristicsByIdAsync(id);
            return _mapper.Map<VirusCharacteristicDto>(result);
        }

        public async Task<IEnumerable<VirusCharacteristicDto>> GetAllVirusCharacteristicsByVirusTypeAsync(Guid? virusType, bool isAbscent)
        {
            return _mapper.Map<IEnumerable<VirusCharacteristicDto>>(
                await _virusCharacteristicRepository.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent));
        }

        public async Task AddEntryAsync(VirusCharacteristicDto dto)
        {
            dto.Id = Guid.NewGuid();
            await _virusCharacteristicRepository.AddEntryAsync(_mapper.Map<VirusCharacteristic>(dto));
        }

        public async Task UpdateEntryAsync(VirusCharacteristicDto dto)
        {
            await _virusCharacteristicRepository.UpdateEntryAsync(_mapper.Map<VirusCharacteristic>(dto));
        }

        public async Task DeleteVirusCharactersticsAsync(Guid id, byte[] lastModified)
        {
            await _virusCharacteristicRepository.DeleteVirusCharactersticsAsync(id, lastModified);
        }
        
        public async Task<bool> CheckVirusCharactersticsUsageByIdAsync(Guid id)
        {
            return await _virusCharacteristicRepository.CheckVirusCharactersticsUsageByIdAsync(id);
        }
        
        public async Task<IEnumerable<VirusCharacteristicDataTypeDto>> GetAllVirusCharactersticsTypeNamesAsync()
        {
            return _mapper.Map<IEnumerable<VirusCharacteristicDataTypeDto>>(await _virusCharacteristicRepository.GetAllVirusCharactersticsTypeNamesAsync());
        }
    }
}
