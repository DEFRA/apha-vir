using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;

namespace Apha.VIR.DataAccess.Repositories;

public class VirusTypeCharacteristicRepository : IVirusTypeCharacteristicRepository
{
    private readonly VIRDbContext _context;

    public VirusTypeCharacteristicRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

}
