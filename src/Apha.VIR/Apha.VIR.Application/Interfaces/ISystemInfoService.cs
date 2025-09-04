using Apha.VIR.Application.DTOs;


namespace Apha.VIR.Application.Interfaces
{
    public interface ISystemInfoService
    {
        Task<SystemInfoDTO> GetLatestSysInfo();

        Task<string> GetEnvironmentName();
    }
}