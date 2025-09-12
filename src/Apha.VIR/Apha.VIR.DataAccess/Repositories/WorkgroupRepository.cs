using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class WorkgroupRepository : RepositoryBase<Workgroup>, IWorkgroupRepository
{
    public WorkgroupRepository(VIRDbContext context): base(context) { }

    public async Task<IEnumerable<Workgroup>> GetWorkgroupfListAsync()
    {
        return await GetDbSetFor<Workgroup>().ToListAsync();
    }
}
