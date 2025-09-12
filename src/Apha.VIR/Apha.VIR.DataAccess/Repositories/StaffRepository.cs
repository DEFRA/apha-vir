using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class StaffRepository : RepositoryBase<Staff>, IStaffRepository
{
    public StaffRepository(VIRDbContext context) : base(context) { }

    public async Task<IEnumerable<Staff>> GetStaffListAsync()
    {
        return await GetDbSetFor<Staff>().ToListAsync();
    }
}
