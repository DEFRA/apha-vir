using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolatesService
    {
        Task<IsolateFullDetailDTO> GetIsolateFullDetailsAsync(Guid IsolateId);
        Task<IEnumerable<IsolateCharacteristicInfoDTO>> GetIsolateCharacteristicInfoAsync(Guid IsolateId);
        Task UpdateIsolateCharacteristicsAsync(IsolateCharacteristicInfoDTO item, string User);
    }
}
