using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class IsolateRelocateService : IIsolateRelocateService
    {
        private readonly IIsolateRelocateRepository _isolateRelocateRepository;
        private readonly IMapper _mapper;
        public IsolateRelocateService(IIsolateRelocateRepository 
            isolateRelocateRepository, IMapper mapper)
        {
            _isolateRelocateRepository = isolateRelocateRepository ?? throw new ArgumentNullException(nameof(isolateRelocateRepository));
            _mapper = mapper;
        }
        public async Task<IEnumerable<IsolateRelocateDTO>> GetIsolatesByCriteria(string min, string max, Guid? freezer, Guid? tray)
        {
            var isolateDetail = await _isolateRelocateRepository.GetIsolatesByCriteria(min, max, freezer, tray);
            return _mapper.Map<IEnumerable<IsolateRelocateDTO>>(isolateDetail);           
        }

        public async Task UpdateIsolateFreezeAndTrayAsync(IsolateRelocateDTO item)
        {
            var isolateDetail = _mapper.Map<IsolateRelocate>(item);
            await _isolateRelocateRepository.UpdateIsolateFreezeAndTrayAsync(isolateDetail);
        }
    }
}
