using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface IIsolateRepository
{
    Task<IEnumerable<IsolateInfo>> GetIsolateInfoByAVNumberAsync(string AVNumber);
    Task<IsolateFullDetail> GetIsolateFullDetailsByIdAsync(Guid isolateId);
}
