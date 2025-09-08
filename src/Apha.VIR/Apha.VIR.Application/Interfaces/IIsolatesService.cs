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
        Task DeleteIsolateAsync(Guid isolateId, string userId, byte[] lastModified);
        Task<string> GenerateNomenclature(string avNumber, Guid sampleId, string virusType, string yearOfIsolation);
        Task<IEnumerable<IsolateCharacteristicDTO>> GetIsolateCharacteristicInfoAsync(Guid IsolateId);
        Task UpdateIsolateCharacteristicsAsync(IsolateCharacteristicDTO item, string User);
        Task<IEnumerable<IsolateInfoDTO>> GetIsolateInfoByAVNumberAsync(string AVNumber);
        Task<bool> UniqueNomenclatureAsync(Guid isolateId, string? familyName, Guid type);
    }
}
