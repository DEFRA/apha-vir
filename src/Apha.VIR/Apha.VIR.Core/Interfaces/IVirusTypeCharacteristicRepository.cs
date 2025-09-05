namespace Apha.VIR.Core.Interfaces;

public interface IVirusTypeCharacteristicRepository
{
    Task AssignCharacteristicToTypeAsync(Guid virusTypeId, Guid characteristicId);
    Task RemoveCharacteristicFromTypeAsync(Guid virusTypeId, Guid characteristicId);
}
