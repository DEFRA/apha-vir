using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class VirusCharacteristicAssociationRepository : IVirusCharacteristicAssociationRepository
{
    private readonly VIRDbContext _context;
    public VirusCharacteristicAssociationRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task AssignCharacteristicToTypeAsync(Guid virusTypeId, Guid characteristicId)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC spVirusCharacteristicLinkUpdate @VirusType = {virusTypeId}, @VirusCharacteristic = {characteristicId}, @Mode = {"Assign"}");
    }

    public async Task RemoveCharacteristicFromTypeAsync(Guid virusTypeId, Guid characteristicId)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC spVirusCharacteristicLinkUpdate @VirusType = {virusTypeId}, @VirusCharacteristic = {characteristicId}, @Mode = {"Remove"}");
    }
}
