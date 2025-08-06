using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class WorkgroupRepository : IWorkgroupRepository
{
    private readonly VIRDbContext _context;

    public WorkgroupRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Workgroup>> GetWorkgroupfListAsync()
    {
        return await _context.Workgroups.ToListAsync();
    }
}
