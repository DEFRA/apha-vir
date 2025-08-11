using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class IsolateViabilityRepository : IIsolateViabilityRepository
{

    private readonly VIRDbContext _context;

    public IsolateViabilityRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task<IEnumerable<IsolateViability>> GetViabilityByIsolateIdAsync(Guid isolateId)
    {

            var result = await _context.IsolateViabilities
                .FromSqlRaw("EXEC spIsolateViabilityGetByIsolateId @IsolateID",
                    new Microsoft.Data.SqlClient.SqlParameter("@IsolateID", isolateId))
                .ToListAsync();

            return result;

    }
}
