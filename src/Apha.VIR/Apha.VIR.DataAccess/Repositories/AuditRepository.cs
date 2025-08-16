using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly VIRDbContext _context;

    public AuditRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<AuditSubmissionLog>> GetSubmissionLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
    {
        SqlParameter[] parameters = GetSqlParameters(avNumber, dateFrom, dateTo, userid);

        return await _context.Set<AuditSubmissionLog>()
           .FromSqlRaw("EXEC spLogSubmissionGetBySearch @AVNumber,@DateFrom,@DateTo,@UserId"
           , parameters).ToListAsync();
    }

    public async Task<IEnumerable<AuditCharacteristicLog>> GetCharacteristicsLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
    {
        SqlParameter[] parameters = GetSqlParameters(avNumber, dateFrom, dateTo, userid);

        return await _context.Set<AuditCharacteristicLog>()
            .FromSqlRaw("EXEC spLogCharacteristicsGetBySearch @AVNumber,@DateFrom,@DateTo,@UserId"
            , parameters).ToListAsync();
    }

    public async Task<IEnumerable<AuditDispatchLog>> GetDispatchLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
    {
        SqlParameter[] parameters = GetSqlParameters(avNumber, dateFrom, dateTo, userid);

        return await _context.Set<AuditDispatchLog>()
           .FromSqlRaw("EXEC spLogDispatchGetBySearch @AVNumber,@DateFrom,@DateTo,@UserId"
           , parameters).ToListAsync();
    }

    public async Task<IEnumerable<AuditViabilityLog>> GetIsolateViabilityLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
    {
        SqlParameter[] parameters = GetSqlParameters(avNumber, dateFrom, dateTo, userid);

        return await _context.Set<AuditViabilityLog>()
           .FromSqlRaw("EXEC spLogisolateViabilityGetBySearch @AVNumber,@DateFrom,@DateTo,@UserId"
           , parameters).ToListAsync();
    }

    public async Task<IEnumerable<AuditIsolateLogDetail>> GetIsolatLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
    {
        SqlParameter[] parameters = GetSqlParameters(avNumber, dateFrom, dateTo, userid);

        return await _context.Set<AuditIsolateLogDetail>()
           .FromSqlRaw("EXEC spLogIsolateGetBySearch @AVNumber,@DateFrom,@DateTo,@UserId"
           , parameters).ToListAsync();
    }

    public async Task<IEnumerable<AuditSampleLog>> GetSamplLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
    {
        SqlParameter[] parameters = GetSqlParameters(avNumber, dateFrom, dateTo, userid);

        return await _context.Set<AuditSampleLog>()
           .FromSqlRaw("EXEC spLogSampleGetBySearch @AVNumber,@DateFrom,@DateTo,@UserId"
           , parameters).ToListAsync();
    }

    private static SqlParameter[] GetSqlParameters(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
    {
        return new[]
        {
            new SqlParameter("@AVNumber",avNumber),
            new SqlParameter("@DateFrom",dateFrom),
            new SqlParameter("@DateTo",dateTo),
            new SqlParameter("@UserID", userid)
        };
    }
}
