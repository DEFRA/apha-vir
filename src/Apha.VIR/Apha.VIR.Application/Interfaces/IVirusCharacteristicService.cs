using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface IVirusCharacteristicService
    {
        Task<IEnumerable<VirusCharacteristicDto>> GetAllVirusCharacteristicsAsync();
        Task<IEnumerable<VirusCharacteristicDto>> GetAllVirusCharacteristicsByVirusTypeAsync(Guid? virusType, bool isAbscent);
    }
}