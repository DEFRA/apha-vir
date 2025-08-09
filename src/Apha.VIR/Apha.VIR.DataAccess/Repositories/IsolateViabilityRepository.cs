using System.Data;
using System.Linq;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class IsolateViabilityRepository : IIsolateViabilityRepository
{
    private readonly VIRDbContext _context;

    public IsolateViabilityRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<IsolateViability>> GetViabilityHistoryAsync(Guid IsolateId)
    {

        var parameters = new[]
        {
            new SqlParameter("@IsolateID", IsolateId),
        };

        return (await _context.Set<IsolateViability>()
           .FromSqlRaw($"EXEC spIsolateViabilityGetByIsolateId  @IsolateID ", parameters).ToListAsync());
    }

    public async Task DeleteIsolateViabilityAsync(Guid IsolateId, byte[] lastModified, string userid)
    {

        var parameters = new[]
        {
                new SqlParameter("@IsolateViabilityId",IsolateId),
                new SqlParameter("@UserID", userid),
                new SqlParameter  {
                    ParameterName = "@LastModified",
                    SqlDbType = SqlDbType.Timestamp,
                    Value = lastModified
                   //Direction = ParameterDirection.InputOutput,
                }
            };

        await _context.Database
            .ExecuteSqlRawAsync($"EXEC spIsolateViabilityDelete @UserID, @IsolateViabilityId, @LastModified", parameters);
    }

    public async Task UpdateIsolateViabilityAsync(IsolateViability isolateViability, string userid)
    {
        var parameters = new[]
        {
            new SqlParameter("@IsolateViabilityId",isolateViability.IsolateViabilityId),
            new SqlParameter("@IsolateViabilityIsolateID",isolateViability.IsolateViabilityIsolateId),
            new SqlParameter("@Viable",isolateViability.Viable),
            new SqlParameter("@DateChecked",isolateViability.DateChecked),
            new SqlParameter("@CheckedByID",isolateViability.CheckedById),
                new SqlParameter("@UserID", userid),
                new SqlParameter  {
                    ParameterName = "@LastModified",
                    SqlDbType = SqlDbType.Timestamp,
                    Value = isolateViability.LastModified
                   //Direction = ParameterDirection.InputOutput,
                }
            };

        await _context.Database
            .ExecuteSqlRawAsync($"EXEC spIsolateViabilityUpdate @UserID, @IsolateViabilityId," +
            $" @IsolateViabilityIsolateID, @Viable, @DateChecked, @CheckedByID, @LastModified", parameters);
    }
}
