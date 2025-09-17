using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolateDispatchService
    {
        Task<IsolateInfoDto> GetIsolateInfoByAVNumberAndIsolateIdAsync(string AVNumber, Guid IsolateId);
        Task<IEnumerable<IsolateDispatchInfoDto>> GetDispatchesHistoryAsync(string AVNumber, Guid IsolateId);
        Task<IsolateDispatchInfoDto> GetDispatchForIsolateAsync(string AVNumber, Guid DispatchId, Guid DispatchIsolateId);
        Task<IsolateFullDetailDto> GetDispatcheConfirmationAsync(Guid IsolateId);
        Task AddDispatchAsync(IsolateDispatchInfoDto DispatchInfo, string User);
        Task DeleteDispatchAsync(Guid DispatchId, byte[] LastModified, string User);
        Task UpdateDispatchAsync(IsolateDispatchInfoDto DispatchInfoDto, string User);
        Task<IsolateViabilityDto?> GetLastViabilityByIsolateAsync(Guid IsolateId);
        Task<int> GetIsolateDispatchRecordCountAsync(Guid isolateId);
    }
}
