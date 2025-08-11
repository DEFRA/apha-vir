using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolateDispatchService
    {
        Task<IEnumerable<IsolateDispatchInfoDTO>> GetDispatchesHistoryAsync(string AVNumber, Guid IsolateId);
        Task<IsolateDispatchInfoDTO> GetDispatchForIsolateAsync(string AVNumber, Guid DispatchId, Guid DispatchIsolateId);
        Task<IsolateFullDetailDTO> GetDispatcheConfirmationAsync(Guid IsolateId);
        Task DeleteDispatchAsync(Guid DispatchId, byte[] LastModified, string User);
        Task UpdateDispatchAsync(IsolateDispatchInfoDTO DispatchInfoDto, string User);
    }
}
