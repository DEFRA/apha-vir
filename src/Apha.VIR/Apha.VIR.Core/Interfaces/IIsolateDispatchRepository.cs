using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface IIsolateDispatchRepository
{
    Task<IEnumerable<IsolateDispatchInfo>> GetDispatchesHistoryAsync(Guid IsolateId);
    Task AddDispatchAsync(IsolateDispatchInfo DispatchInfo, string User);
    Task DeleteDispatchAsync(Guid DispatchId, Byte[] LastModified, string User);
    Task UpdateDispatchAsync(IsolateDispatchInfo DispatchInfo, string User);
    Task<int> GetIsolateDispatchRecordCountAsync(Guid isolateId);
}
