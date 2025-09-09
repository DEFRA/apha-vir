namespace Apha.VIR.Core.Interfaces;

public interface IVirusCharacteristicAssociationRepository
{
    Task AssignCharacteristicToTypeAsync(Guid virusTypeId, Guid characteristicId);
    Task RemoveCharacteristicFromTypeAsync(Guid virusTypeId, Guid characteristicId);
}
