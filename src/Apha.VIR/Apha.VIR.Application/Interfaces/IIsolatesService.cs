using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolatesService
    {
        Task<IsolateFullDetailDTO> GetIsolateFullDetailsAsync(Guid IsolateId);
        Task<Guid> AddIsolateDetailsAsync(IsolateDTO isolate);
        Task<IsolateDTO> GetIsolateByIsolateAndAVNumberAsync(string avNumber, Guid isolateId);
        Task UpdateIsolateDetailsAsync(IsolateDTO isolate);
        Task<string> GenerateNomenclature(string avNumber, Guid sampleId, string virusType, string yearOfIsolation);
        Task<IEnumerable<IsolateCharacteristicInfoDTO>> GetIsolateCharacteristicInfoAsync(Guid IsolateId);
        Task UpdateIsolateCharacteristicsAsync(IsolateCharacteristicInfoDTO item, string User);
    }
}
