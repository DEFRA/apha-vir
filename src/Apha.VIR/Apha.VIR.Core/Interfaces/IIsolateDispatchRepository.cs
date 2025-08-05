using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface IIsolateDispatchRepository
{
    Task<IEnumerable<IsolateDispatchInfo>> GetDispatchesHistoryAsync(Guid IsolateId);
    Task DeleteDispatchAsync(Guid DispatchId, Byte[] LastModified, string User);
    //Task<IEnumerable<IsolateDispatch>> GetDispatchAsync(Guid isolateId);
}
