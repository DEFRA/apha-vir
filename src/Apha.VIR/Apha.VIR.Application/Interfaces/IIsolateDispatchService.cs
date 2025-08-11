using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolateDispatchService
    {
        Task<IEnumerable<IsolateDispatchInfoDTO>> GetDispatchesHistoryAsync(string AVNumber, Guid IsolateId);

        Task DeleteDispatchAsync(Guid DispatchId, byte[] LastModified, string User);

        Task<IsolateDispatchInfoDTO> GetDispatchForIsolateAsync(string AVNumber, Guid DispatchId, Guid DispatchIsolateId);

        Task UpdateDispatchAsync(IsolateDispatchInfoDTO DispatchInfoDto, string User);

        Task<IsolateFullDetailDTO> GetDispatcheConfirmationAsync(Guid IsolateId);
    }
}
