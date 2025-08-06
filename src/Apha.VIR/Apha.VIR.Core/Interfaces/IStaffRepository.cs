using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface IStaffRepository
{
    Task<IEnumerable<Staff>> GetStaffListAsync();
}
