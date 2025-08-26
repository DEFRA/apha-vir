using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace YourNamespace.Services
{
    public class SystemInfoService : ISystemInfoService
    {
        private readonly ISystemInfoRepository _sysInfoRepository;
        private readonly IMapper _mapper;

        public SystemInfoService(ISystemInfoRepository sysInfoRepository, IMapper mapper)
        {
            _sysInfoRepository = sysInfoRepository ?? throw new ArgumentNullException(nameof(sysInfoRepository));
            _mapper = mapper;
        }

        public async Task<SystemInfoDTO> GetLatestSysInfo()
        {
            return _mapper.Map<SystemInfoDTO>(await _sysInfoRepository.GetLatestSysInfoAsync());
        }
    }
}