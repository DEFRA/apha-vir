using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;

namespace Apha.VIR.DataAccess.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly VIRDbContext _context;

    public AuditRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
}
