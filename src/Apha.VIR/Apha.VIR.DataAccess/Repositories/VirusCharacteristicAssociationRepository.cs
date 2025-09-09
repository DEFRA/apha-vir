using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;

namespace Apha.VIR.DataAccess.Repositories;

public class VirusCharacteristicAssociationRepository : RepositoryBase<object>, IVirusCharacteristicAssociationRepository
{
    public VirusCharacteristicAssociationRepository(VIRDbContext context): base(context)
    {
    }
    public async Task AssignCharacteristicToTypeAsync(Guid virusTypeId, Guid characteristicId)
    {
        await ExecuteSqlInterpolatedAsync(
            $"EXEC spVirusCharacteristicLinkUpdate @VirusType = {virusTypeId}, @VirusCharacteristic = {characteristicId}, @Mode = {"Assign"}");
    }

    public async Task RemoveCharacteristicFromTypeAsync(Guid virusTypeId, Guid characteristicId)
    {
        await ExecuteSqlInterpolatedAsync(
            $"EXEC spVirusCharacteristicLinkUpdate @VirusType = {virusTypeId}, @VirusCharacteristic = {characteristicId}, @Mode = {"Remove"}");
    }
}
