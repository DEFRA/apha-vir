using System.Data;
using System.Linq;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class IsolateViabilityRepository : RepositoryBase<IsolateViability>, IIsolateViabilityRepository
{
    public IsolateViabilityRepository(VIRDbContext context) : base(context) { }
    public async Task<IEnumerable<IsolateViability>> GetViabilityHistoryAsync(Guid IsolateId)
    {

        var parameters = new[]
        {
            new SqlParameter("@IsolateID", IsolateId),
        };

        return (await GetQueryableResultFor<IsolateViability>($"EXEC spIsolateViabilityGetByIsolateId  @IsolateID ", parameters).ToListAsync());
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

        await ExecuteSqlAsync($"EXEC spIsolateViabilityDelete @UserID, @IsolateViabilityId, @LastModified", parameters);
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

        await ExecuteSqlAsync($"EXEC spIsolateViabilityUpdate @UserID, @IsolateViabilityId," +
            $" @IsolateViabilityIsolateID, @Viable, @DateChecked, @CheckedByID, @LastModified", parameters);
    }

    public async Task<IEnumerable<IsolateViability>> GetViabilityByIsolateIdAsync(Guid isolateId)
    {

        var result = await GetQueryableResultFor<IsolateViability>("EXEC spIsolateViabilityGetByIsolateId @IsolateID",
                new Microsoft.Data.SqlClient.SqlParameter("@IsolateID", isolateId))
            .ToListAsync();

        return result;

    }

    public async Task AddIsolateViabilityAsync(IsolateViability isolateViability, string userId)
    {
        var parameters = new[]
        {
            new SqlParameter("@IsolateViabilityId", Guid.NewGuid()),
            new SqlParameter("@IsolateViabilityIsolateID",isolateViability.IsolateViabilityIsolateId),
            new SqlParameter("@Viable",isolateViability.Viable),
            new SqlParameter("@DateChecked",isolateViability.DateChecked),
            new SqlParameter("@CheckedByID",isolateViability.CheckedById),
                new SqlParameter("@UserID", userId),
                new SqlParameter  {
                    ParameterName = "@LastModified",
                    SqlDbType = SqlDbType.Timestamp,
                    Value = isolateViability.LastModified,
                    Direction = ParameterDirection.InputOutput,
                }
            };

        await ExecuteSqlAsync($"EXEC spIsolateViabilityInsert @UserID, @IsolateViabilityId, " +
            $"@IsolateViabilityIsolateID, @Viable, @DateChecked, @CheckedByID, @LastModified OUTPUT", parameters);
    }
}
