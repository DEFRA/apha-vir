using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly VIRDbContext _context;

    public StaffRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Staff>> GetStaffListAsync()
    {
        return await _context.Staffs.ToListAsync();
    }
}
