using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolatesService
    {
        Task<IsolateFullDetailDto> GetIsolateFullDetailsAsync(Guid IsolateId);
        Task<Guid> AddIsolateDetailsAsync(IsolateDto isolate);
        Task<IsolateDto> GetIsolateByIsolateAndAVNumberAsync(string avNumber, Guid isolateId);
        Task UpdateIsolateDetailsAsync(IsolateDto isolate);
        Task DeleteIsolateAsync(Guid isolateId, string userId, byte[] lastModified);
        Task<string> GenerateNomenclature(string avNumber, Guid sampleId, string virusType, string yearOfIsolation);
        Task<IEnumerable<IsolateCharacteristicDto>> GetIsolateCharacteristicInfoAsync(Guid IsolateId);
        Task UpdateIsolateCharacteristicsAsync(IsolateCharacteristicDto item, string User);
        Task<IEnumerable<IsolateInfoDto>> GetIsolateInfoByAVNumberAsync(string AVNumber);
        Task<bool> UniqueNomenclatureAsync(Guid isolateId, string? familyName, Guid type);
    }
}
