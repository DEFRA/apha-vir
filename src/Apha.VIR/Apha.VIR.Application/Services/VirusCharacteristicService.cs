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
        public async Task<IEnumerable<VirusCharacteristicDTO>> GetAllVirusCharacteristicsAsync()
        {
            var result = await _virusCharacteristicRepository.GetAllVirusCharacteristicsAsync();
            return _mapper.Map<IEnumerable<VirusCharacteristicDTO>>(result);
        }
        public async Task<PaginatedResult<VirusCharacteristicDTO>> GetAllVirusCharacteristicsAsync(int pageNo, int pageSize)
        {
            var result = await _virusCharacteristicRepository.GetAllVirusCharacteristicsAsync(pageNo, pageSize);
            return _mapper.Map<PaginatedResult<VirusCharacteristicDTO>>(result);
        }

        public async Task<IEnumerable<VirusCharacteristicDTO>> GetAllVirusCharacteristicsByVirusTypeAsync(Guid? virusType, bool isAbscent)
        {
            return _mapper.Map<IEnumerable<VirusCharacteristicDTO>>(await _virusCharacteristicRepository.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent));
        }
        public async Task AddEntryAsync(VirusCharacteristicDTO dto)
        {
            dto.Id = Guid.NewGuid();
            await _virusCharacteristicRepository.AddEntryAsync(_mapper.Map<VirusCharacteristic>(dto));
        }
        public async Task<IEnumerable<VirusCharacteristicDataTypeDTO>> GetAllVirusCharactersticsTypeNamesAsync()
        {
            return _mapper.Map<IEnumerable<VirusCharacteristicDataTypeDTO>>(await _virusCharacteristicRepository.GetAllVirusCharactersticsTypeNamesAsync());
        }
    }
}
