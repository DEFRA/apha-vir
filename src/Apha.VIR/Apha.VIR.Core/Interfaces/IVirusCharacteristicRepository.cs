using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces
{
    public interface IVirusCharacteristicRepository
    {
        Task<IEnumerable<VirusCharacteristic>> GetAllVirusCharacteristicsAsync();
        Task<IEnumerable<VirusCharacteristic>> GetAllVirusCharacteristicsByVirusTypeAsync(Guid? virusType, bool isAbscent);
    }
}
