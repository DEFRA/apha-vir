using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class VirusCharacteristicListEntryRepository : IVirusCharacteristicListEntryRepository
{
    private readonly VIRDbContext _context;

    public VirusCharacteristicListEntryRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<VirusCharacteristicListEntry>> GetVirusCharacteristicListEntryByVirusCharacteristic(Guid virusCharacteristicId)
    {
        return await _context.Set<VirusCharacteristicListEntry>()
              .FromSqlInterpolated($"EXEC spVirusCharacteristicListEntryGetById @VirusCharacteristicId = {virusCharacteristicId}").ToListAsync();
    }
}