using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories
{
    public class SystemInfoRepository : ISystemInfoRepository
    {
        private readonly VIRDbContext _context;

        public SystemInfoRepository(VIRDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<SystemInfo> GetLatestSysInfoAsync()
        {
            var sysInfoList = await ExecuteStoredProcedureAsync();
            return sysInfoList.SingleOrDefault() ?? throw new InvalidOperationException("No SystemInfo found.");
        }

        public virtual async Task<List<SystemInfo>> ExecuteStoredProcedureAsync()
        {
            return await _context.SystemInfos
            .FromSqlRaw("EXEC spSysInfoGetLatest")
            .AsNoTracking()
            .ToListAsync();
        }
    }
}