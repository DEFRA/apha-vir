using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolateDispatchService
    {
        Task<IsolateInfoDTO> GetIsolateInfoByAVNumberAndIsolateIdAsync(string AVNumber, Guid IsolateId);
        Task<IEnumerable<IsolateDispatchInfoDTO>> GetDispatchesHistoryAsync(string AVNumber, Guid IsolateId);
        Task<IsolateDispatchInfoDTO> GetDispatchForIsolateAsync(string AVNumber, Guid DispatchId, Guid DispatchIsolateId);
        Task<IsolateFullDetailDTO> GetDispatcheConfirmationAsync(Guid IsolateId);
        Task AddDispatchAsync(IsolateDispatchInfoDTO DispatchInfo, string User);
        Task DeleteDispatchAsync(Guid DispatchId, byte[] LastModified, string User);
        Task UpdateDispatchAsync(IsolateDispatchInfoDTO DispatchInfoDto, string User);
        Task<IsolateViabilityDTO?> GetLastViabilityByIsolateAsync(Guid IsolateId);
        Task<int> GetIsolateDispatchRecordCountAsync(Guid isolateId);
    }
}
