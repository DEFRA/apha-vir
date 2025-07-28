using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;

namespace Apha.VIR.DataAccess.Repositories;

public class IsolateRelocateRepository : IIsolateRelocateRepository
{
    private readonly VIRDbContext _context;

    public IsolateRelocateRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
}