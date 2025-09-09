using System;
using System.Data;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class IsolateRelocateRepository : IIsolateRelocateRepository
{
    private readonly VIRDbContext _context;
    public IsolateRelocateRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<IsolateRelocate>> GetIsolatesByCriteria(string min, string max, Guid? freezer, Guid? tray)
    {
        var parameters = new[]
        {
            new SqlParameter("@MinAVNumber", SqlDbType.VarChar, 11) { Value = string.IsNullOrEmpty(min) ? DBNull.Value : min },
            new SqlParameter("@MaxAVNumber", SqlDbType.VarChar, 11) { Value = string.IsNullOrEmpty(max) ? DBNull.Value : max},
            new SqlParameter("@Freezer", SqlDbType.UniqueIdentifier) { Value = freezer == Guid.Empty ? DBNull.Value : freezer},
            new SqlParameter("@Tray",  SqlDbType.UniqueIdentifier) { Value = tray == Guid.Empty ? DBNull.Value : tray }
           
        };
        return await _context.Database.SqlQueryRaw<IsolateRelocate>(
            "EXEC spIsolateRelocateGetByCriteria @MinAVNumber, @MaxAVNumber, @Freezer, @Tray", parameters).ToListAsync();

    }
    public async Task UpdateIsolateFreezeAndTrayAsync(IsolateRelocate item)
    {
        await _context.Database.ExecuteSqlRawAsync(
          "EXEC spIsolateRelocateByIsolate @UserID, @IsolateId, @Freezer, @Tray, @Well, @LastModified OUTPUT, @FreezerName OUTPUT, @TrayName OUTPUT",
                       new SqlParameter("@UserID", SqlDbType.VarChar, 20) { Value = item.UserID },
                       new SqlParameter("@IsolateId", SqlDbType.UniqueIdentifier) { Value = item.IsolateId },
                       new SqlParameter("@Freezer", SqlDbType.UniqueIdentifier) { Value = item.Freezer },
                       new SqlParameter("@Tray", SqlDbType.UniqueIdentifier) { Value = item.Tray },
                       new SqlParameter("@Well", SqlDbType.VarChar, 10) { Value = item.Well },
                       new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = (object?)item.LastModified ?? DBNull.Value, Direction = ParameterDirection.InputOutput },
                       new SqlParameter("@FreezerName", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output },
                       new SqlParameter("@TrayName", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });

    }
}