namespace Apha.VIR.Application.Interfaces
{
    public interface IVirusCharacteristicAssociationService
    {
        Task AssignCharacteristicToTypeAsync(Guid virusTypeId, Guid characteristicId);
        Task RemoveCharacteristicFromTypeAsync(Guid virusTypeId, Guid characteristicId);
    }
}
