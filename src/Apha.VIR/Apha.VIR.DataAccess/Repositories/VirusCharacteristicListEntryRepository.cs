using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;

namespace Apha.VIR.DataAccess.Repositories;

public class VirusCharacteristicListEntryRepository : IVirusCharacteristicListEntryRepository
{
    private readonly VIRDbContext _context;

    public VirusCharacteristicListEntryRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
 
}
