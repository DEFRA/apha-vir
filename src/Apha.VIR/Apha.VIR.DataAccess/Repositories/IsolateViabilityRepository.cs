using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;

namespace Apha.VIR.DataAccess.Repositories;

public class IsolateViabilityRepository : IIsolateViabilityRepository
{
    private readonly VIRDbContext _context;

    public IsolateViabilityRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
}
