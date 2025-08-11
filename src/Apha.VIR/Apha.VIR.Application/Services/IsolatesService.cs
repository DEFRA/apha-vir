using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class IsolatesService : IIsolatesService
    {
        private readonly IIsolateRepository _iIsolateRepository;
        private readonly IMapper _mapper;

        public IsolatesService(IIsolateRepository iIsolateRepository,
            IMapper mapper)
        {
            _iIsolateRepository = iIsolateRepository ?? throw new ArgumentNullException(nameof(iIsolateRepository));
            _mapper = mapper;
        }

        public async Task<IsolateFullDetailDTO> GetIsolateFullDetailsAsync(Guid IsolateId)
        {
            var isolateFullDetail = await _iIsolateRepository.GetIsolateFullDetailsByIdAsync(IsolateId);
            return _mapper.Map<IsolateFullDetailDTO>(isolateFullDetail);
        }
    }
}
