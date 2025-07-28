using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;

namespace Apha.VIR.DataAccess.Repositories;

public class IsolateRepository : IIsolateRepository
{
    private readonly VIRDbContext _context;

    public IsolateRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
}
