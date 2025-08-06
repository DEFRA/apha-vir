using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolateDispatchService
    {
        Task<IEnumerable<IsolateDispatchInfoDTO>> GetDispatchesHistoryAsync(string AVNumber, Guid IsolateId);
        Task DeleteDispatchAsync(Guid DispatchId, byte[] LastModified, string User);
        
    }
}
