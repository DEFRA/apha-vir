using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
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
            return _mapper.Map<IEnumerable<VirusCharacteristicDto>>(await _virusCharacteristicRepository.GetAllVirusCharacteristicsAsync());
        }

        public async Task<IEnumerable<VirusCharacteristicDto>> GetAllVirusCharacteristicsByVirusTypeAsync(Guid? virusType, bool isAbscent)
        {
            return _mapper.Map<IEnumerable<VirusCharacteristicDto>>(await _virusCharacteristicRepository.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent));
        }
    }
}
