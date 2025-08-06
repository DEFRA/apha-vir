using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface IWorkgroupRepository
{
    Task<IEnumerable<Workgroup>> GetWorkgroupfListAsync();
}
