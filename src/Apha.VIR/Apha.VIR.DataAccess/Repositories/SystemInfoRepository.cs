using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;

namespace Apha.VIR.DataAccess.Repositories;

public class SystemInfoRepository : ISystemInfoRepository
{
    private readonly VIRDbContext _context;

    public SystemInfoRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
  
}
