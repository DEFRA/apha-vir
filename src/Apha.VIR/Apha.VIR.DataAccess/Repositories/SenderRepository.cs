using System.Data;
using System.Linq;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class SenderRepository : ISenderRepository
{
    private readonly VIRDbContext _context;

    public SenderRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Sender>> GetAllSenderOrderBySenderAsync(Guid? countryId)
    {
        var senders = await _context.Set<Sender>()
        .FromSqlRaw($"EXEC spSenderGetAllOrderBySender").ToListAsync();
        if (countryId != null && countryId != Guid.Empty)
        {
            senders = senders.Where(s => s.Country == countryId).ToList();
        }
        return senders;
    }

    public async Task<IEnumerable<Sender>> GetAllSenderOrderByOrganisationAsync(Guid? countryId)
    {
        var senders = await _context.Set<Sender>()
        .FromSqlRaw($"EXEC spSenderGetAllOrderByOrganisation").ToListAsync();
        if (countryId != null && countryId != Guid.Empty)
        {
            senders = senders.Where(s => s.Country == countryId).ToList();
        }
        return senders;
    }

    public async Task<PagedData<Sender>> GetAllSenderAsync(int pageNo, int pageSize)
    {
        //stored procedure is non-composable SQL and EF does support AsQueryable to get performance of skip. 
        var result = await _context.Set<Sender>()
           .FromSqlInterpolated($"EXEC spSenderGetAllOrderBySender").ToListAsync();

        var totalRecords = result.Count;
        var senders = result.Skip((pageNo - 1) * pageSize)
            .Take(pageSize).ToList();

        return new PagedData<Sender>(senders, totalRecords);
    }

    public async Task AddSenderAsync(Sender sender)
    {
        var parameters = new[]
        {
           new SqlParameter("@SenderID", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() },
           new SqlParameter("@Sender", SqlDbType.VarChar, 50) { Value = (object?)sender.SenderName ?? DBNull.Value  },
           new SqlParameter("@SenderOrganisation", SqlDbType.VarChar, 200) { Value = (object?)sender.SenderOrganisation ?? DBNull.Value },
           new SqlParameter("@SenderAddress", SqlDbType.VarChar, 500) { Value = (object?)sender.SenderAddress ?? DBNull.Value },
           new SqlParameter("@Country", SqlDbType.UniqueIdentifier) { Value = sender.Country.HasValue ? sender.Country : DBNull.Value }
        };

        await _context.Database.ExecuteSqlRawAsync(
            @"EXEC spSenderInsert @SenderID, @Sender, @SenderOrganisation, @SenderAddress, @Country", parameters);
    }


}
