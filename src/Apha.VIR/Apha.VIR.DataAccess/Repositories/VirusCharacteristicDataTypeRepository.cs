using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;

namespace Apha.VIR.DataAccess.Repositories;

public class VirusCharacteristicDataTypeRepository : IVirusCharacteristicDataTypeRepository
{
    private readonly VIRDbContext _context;

    public VirusCharacteristicDataTypeRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
}
