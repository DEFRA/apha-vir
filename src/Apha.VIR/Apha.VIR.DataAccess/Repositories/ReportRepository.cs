using System.Data;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class ReportRepository : RepositoryBase<IsolateDispatchInfo>, IReportRepository
{
    public ReportRepository(VIRDbContext context) : base(context)
    {
    }
    public async Task<IEnumerable<IsolateDispatchInfo>> GetDispatchesReportAsync(DateTime? dateFrom, DateTime? dateTo)
    {
        var dispatchList = new List<IsolateDispatchInfo>();

        using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "spIsolateDispatchGetByDateRange";
                command.CommandType = CommandType.StoredProcedure;

                var paramDateFrom = command.CreateParameter();
                paramDateFrom.ParameterName = "@DateFrom";
                paramDateFrom.Value = dateFrom == null ? DBNull.Value : dateFrom;

                var paramDateTo = command.CreateParameter();
                paramDateTo.ParameterName = "@DateTo";
                paramDateTo.Value = dateTo == null ? DBNull.Value : dateTo;

                command.Parameters.Add(paramDateFrom);
                command.Parameters.Add(paramDateTo);

                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        var dto = new IsolateDispatchInfo
                        {
                            DispatchId = result["DispatchId"] as Guid?,
                            AVNumber = result["AVNumber"] as string,
                            Nomenclature = result["Nomenclature"] as string,
                            IsolateNomenclature = result["IsolateNomenclature"] as string,
                            FamilyName = result["FamilyName"] as string,
                            TypeName = result["TypeName"] as string,
                            IsolateId = result["DispatchIsolateId"] as Guid?,
                            DispatchIsolateId = result["DispatchIsolateId"] as Guid?,
                            NoOfAliquots = result.GetInt32("NoOfAliquots"),
                            PassageNumber = result["PassageNumber"] as int?,
                            RecipientId = result["Recipient"] as Guid?,
                            RecipientName = result["RecipientName"] as string,
                            RecipientAddress = result["RecipientAddress"] as string,
                            ReasonForDispatch = result["ReasonForDispatch"] as string,
                            DispatchedDate = result["DispatchedDate"] as DateTime?,
                            DispatchedById = result["DispatchedBy"] as Guid?,
                            LastModified = result["LastModified"] as Byte[]
                        };
                        dispatchList.Add(dto);
                    }
                }
            }
        }
        return dispatchList;
    }
}
