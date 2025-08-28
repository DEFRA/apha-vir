using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces
{
    public interface ISystemInfoRepository
    {
        Task<SystemInfo> GetLatestSysInfoAsync();
    }
}