using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class IsolatesService : IIsolatesService
    {
        private readonly IIsolateRepository _iIsolateRepository;
        private readonly ICharacteristicRepository _iCharacteristicRepository;
        private readonly IMapper _mapper;

        public IsolatesService(IIsolateRepository iIsolateRepository,
            ICharacteristicRepository iCharacteristicRepository,
            IMapper mapper)
        {
            _iIsolateRepository = iIsolateRepository ?? throw new ArgumentNullException(nameof(iIsolateRepository));
            _iCharacteristicRepository = iCharacteristicRepository ?? throw new ArgumentNullException(nameof(iCharacteristicRepository));
            _mapper = mapper;
        }

        public async Task<IsolateFullDetailDTO> GetIsolateFullDetailsAsync(Guid IsolateId)
        {
            var isolateFullDetail = await _iIsolateRepository.GetIsolateFullDetailsByIdAsync(IsolateId);
            return _mapper.Map<IsolateFullDetailDTO>(isolateFullDetail);
        }

        public async Task<IEnumerable<IsolateCharacteristicInfoDTO>> GetIsolateCharacteristicInfoAsync(Guid IsolateId)
        {           
            var characteristicList = await _iCharacteristicRepository.GetIsolateCharacteristicInfoAsync(IsolateId);
            return _mapper.Map<IEnumerable<IsolateCharacteristicInfoDTO>>(characteristicList);
        }

        public Task UpdateIsolateCharacteristicsAsync(IsolateCharacteristicInfoDTO item, string User)
        {
            var data = _mapper.Map<IsolateCharacteristicInfo>(item);
            return _iCharacteristicRepository.UpdateIsolateCharacteristicsAsync(data, User);
        }
    }
}
