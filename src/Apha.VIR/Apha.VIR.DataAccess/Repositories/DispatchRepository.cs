using System.Data;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class DispatchRepository : IIsolateDispatchRepository
{
    private readonly VIRDbContext _context;

    public DispatchRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<IsolateDispatchInfo>> GetDispatchesHistoryAsync(Guid IsolateId)
    {
        var disPatchresult = (await GetDispatchHistory(IsolateId)).ToList();

        return disPatchresult;
    }

    private async Task<IEnumerable<IsolateDispatchInfo>> GetDispatchHistory(Guid isolateId)
    {
        var dispatchList = new List<IsolateDispatchInfo>();

        using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "spIsolateDispatchGetByIsolateId";
                command.CommandType = CommandType.StoredProcedure;

                var param = command.CreateParameter();
                param.ParameterName = "@IsolateID";
                param.Value = isolateId;
                command.Parameters.Add(param);

                using (var result = await command.ExecuteReaderAsync())
                {
                    int noOfAliquotsToBeDispatchedOrdinal = 2;
                    int noOfAliquotsOrdinal = 30;

                    while (await result.ReadAsync())
                    {
                        var dto = new IsolateDispatchInfo
                        {
                            AVNumber = result["AVNumber"] as string,
                            Nomenclature = result["IsolateNomenclature"] as string,
                            IsolateId = result["IsolateId"] as Guid?,
                            DispatchId = result["DispatchId"] as Guid?,
                            ValidToIssue = !(result["ValidToIssue"] is DBNull) && (bool)result["ValidToIssue"],
                            ViabilityId = result["Viable"] as Guid?,
                            NoOfAliquotsToBeDispatched = result.GetInt32(noOfAliquotsToBeDispatchedOrdinal),
                            NoOfAliquots = result.GetInt32(noOfAliquotsOrdinal),
                            PassageNumber = result["PassageNumber"] as int?,
                            RecipientId = result["Recipient"] as Guid?,
                            RecipientName = result["RecipientName"] as string,
                            RecipientAddress = result["RecipientAddress"] as string,
                            ReasonForDispatch = result["ReasonForDispatch"] as string,
                            DispatchedDate = result["DispatchedDate"] as DateTime?,
                            DispatchedById = result["DispatchedBy"] as Guid?,
                            DispatchIsolateId = result["DispatchIsolateId"] as Guid?,
                            LastModified = result["LastModified"] as Byte[]

                        };
                        dispatchList.Add(dto);

                    }
                }
            }
        }

        return dispatchList;
    }

    public async Task DeleteDispatchAsync(Guid DispatchId, Byte[] LastModified, string User)
    {

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC spDispatchDelete @UserID, @DispatchId, @LastModified OUTPUT",
            new SqlParameter("@UserID", SqlDbType.VarChar, 20) { Value = User },
            new SqlParameter("@DispatchId", SqlDbType.UniqueIdentifier) { Value = DispatchId },
            new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = LastModified, Direction = ParameterDirection.InputOutput }
        );
    }

    public async Task UpdateDispatchAsync(IsolateDispatchInfo DispatchInfo, string User)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC spDispatchUpdate @UserId, @DispatchId, @DispatchIsolateId, @NoOfAliquots, @PassageNumber, @Recipient, @RecipientName, @RecipientAddress, @ReasonForDispatch, @DispatchedDate, @DispatchedById, @LastModified OUTPUT",
            new SqlParameter("@UserId", SqlDbType.VarChar, 20) { Value = User },
            new SqlParameter("@DispatchId", SqlDbType.UniqueIdentifier) { Value = DispatchInfo.DispatchId ?? Guid.Empty },
            new SqlParameter("@DispatchIsolateId", SqlDbType.UniqueIdentifier) { Value = DispatchInfo.DispatchIsolateId ?? Guid.Empty },
            new SqlParameter("@NoOfAliquots", SqlDbType.Int) { Value = DispatchInfo.NoOfAliquotsToBeDispatched },
            new SqlParameter("@PassageNumber", SqlDbType.Int) { Value = (object?)DispatchInfo.PassageNumber ?? DBNull.Value },
            new SqlParameter("@Recipient", SqlDbType.UniqueIdentifier) { Value = (object?)DispatchInfo.RecipientId ?? DBNull.Value },
            new SqlParameter("@RecipientName", SqlDbType.VarChar, 255) { Value = (object?)DispatchInfo.RecipientName ?? DBNull.Value },
            new SqlParameter("@RecipientAddress", SqlDbType.VarChar, 255) { Value = (object?)DispatchInfo.RecipientAddress ?? DBNull.Value },
            new SqlParameter("@ReasonForDispatch", SqlDbType.VarChar, 255) { Value = (object?)DispatchInfo.ReasonForDispatch ?? DBNull.Value },
            new SqlParameter("@DispatchedDate", SqlDbType.DateTime) { Value = (object?)DispatchInfo.DispatchedDate ?? DBNull.Value },
            new SqlParameter("@DispatchedById", SqlDbType.UniqueIdentifier) { Value = (object?)DispatchInfo.DispatchedById ?? DBNull.Value },
            new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = (object?)DispatchInfo.LastModified ?? DBNull.Value, Direction = ParameterDirection.InputOutput }

        );
    }
}
