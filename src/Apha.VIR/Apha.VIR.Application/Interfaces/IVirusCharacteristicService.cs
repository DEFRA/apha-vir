using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface IVirusCharacteristicService
    {
        Task<IEnumerable<VirusCharacteristicDTO>> GetAllVirusCharacteristicsAsync();
        Task<IEnumerable<VirusCharacteristicDTO>> GetAllVirusCharacteristicsByVirusTypeAsync(string? virusType, bool isAbscent);
    }
}
