using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Interfaces;

namespace Apha.VIR.Application.Services
{
    public class VirusCharacteristicAssociationService : IVirusCharacteristicAssociationService
    {
        private readonly IVirusCharacteristicAssociationRepository _repo;
        public VirusCharacteristicAssociationService(IVirusCharacteristicAssociationRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task AssignCharacteristicToTypeAsync(Guid virusTypeId, Guid characteristicId)
        {
            await _repo.AssignCharacteristicToTypeAsync(virusTypeId, characteristicId);
        }

        public async Task RemoveCharacteristicFromTypeAsync(Guid virusTypeId, Guid characteristicId)
        {
            await _repo.RemoveCharacteristicFromTypeAsync(virusTypeId, characteristicId);
        }
    }
}
