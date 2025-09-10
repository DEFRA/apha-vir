using System.Data;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class DispatchRepository : RepositoryBase<IsolateDispatchInfo>, IIsolateDispatchRepository
{
    public DispatchRepository(VIRDbContext context) : base(context) { }

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
                    while (await result.ReadAsync())
                    {
                        var dto = new IsolateDispatchInfo
                        {
                            AVNumber = result["AVNumber"] as string,
                            Nomenclature = result["Nomenclature"] as string,
                            IsolateNomenclature = result["IsolateNomenclature"] as string,
                            IsolateId = result["IsolateId"] as Guid?,
                            DispatchIsolateId = result["DispatchIsolateId"] as Guid?,
                            DispatchId = result["DispatchId"] as Guid?,
                            ValidToIssue = !(result["ValidToIssue"] is DBNull) && (bool)result["ValidToIssue"],
                            ViabilityId = result["Viable"] as Guid?,
                            NoOfAliquots = result.GetInt32("NoOfAliquots"),//dispatch table column
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

    public async Task AddDispatchAsync(IsolateDispatchInfo DispatchInfo, string User)
    {
        var parameters = new[]
        {
           new SqlParameter("@DispatchId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() },
           new SqlParameter("@DispatchIsolateId", SqlDbType.UniqueIdentifier) { Value = DispatchInfo.DispatchIsolateId ?? Guid.Empty },
           new SqlParameter("@NoOfAliquots", SqlDbType.Int) { Value = DispatchInfo.NoOfAliquots },
           new SqlParameter("@PassageNumber", SqlDbType.Int) { Value = (object?)DispatchInfo.PassageNumber ?? DBNull.Value },
           new SqlParameter("@Recipient", SqlDbType.UniqueIdentifier) { Value = (object?)DispatchInfo.RecipientId ?? Guid.Empty },
           new SqlParameter("@RecipientName", SqlDbType.VarChar, 255) { Value = (object?)DispatchInfo.RecipientName ?? DBNull.Value },
           new SqlParameter("@RecipientAddress", SqlDbType.VarChar, 255) { Value = (object?)DispatchInfo.RecipientAddress ?? DBNull.Value },
           new SqlParameter("@ReasonForDispatch", SqlDbType.VarChar, 255) { Value = (object?)DispatchInfo.ReasonForDispatch ?? DBNull.Value },
           new SqlParameter("@DispatchedDate", SqlDbType.DateTime, 20) { Value = (object?)DispatchInfo.DispatchedDate ?? DBNull.Value },
           new SqlParameter("@DispatchedBy", SqlDbType.UniqueIdentifier) { Value = (object?)DispatchInfo.DispatchedById ?? Guid.Empty },
           new SqlParameter("@UserId", SqlDbType.VarChar, 20) { Value = User },
           new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = (object?)DispatchInfo.LastModified ?? DBNull.Value, Direction = ParameterDirection.InputOutput }
        };

        await ExecuteSqlAsync(
           @"EXEC spDispatchInsert @UserID, @DispatchId, @DispatchIsolateId, @NoOfAliquots, @PassageNumber, 
@Recipient, @RecipientName, @RecipientAddress, @ReasonForDispatch, @DispatchedDate, @DispatchedBy, @LastModified OUTPUT",
           parameters);
    }

    public async Task DeleteDispatchAsync(Guid DispatchId, Byte[] LastModified, string User)
    {

        await ExecuteSqlAsync(
            "EXEC spDispatchDelete @UserID, @DispatchId, @LastModified OUTPUT",
            new SqlParameter("@UserID", SqlDbType.VarChar, 20) { Value = User },
            new SqlParameter("@DispatchId", SqlDbType.UniqueIdentifier) { Value = DispatchId },
            new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = LastModified, Direction = ParameterDirection.InputOutput }
        );
    }

    public async Task UpdateDispatchAsync(IsolateDispatchInfo DispatchInfo, string User)
    {
        await ExecuteSqlAsync(
            "EXEC spDispatchUpdate @UserId, @DispatchId, @DispatchIsolateId, @NoOfAliquots, @PassageNumber, @Recipient, @RecipientName, @RecipientAddress, @ReasonForDispatch, @DispatchedDate, @DispatchedById, @LastModified OUTPUT",
            new SqlParameter("@UserId", SqlDbType.VarChar, 20) { Value = User },
            new SqlParameter("@DispatchId", SqlDbType.UniqueIdentifier) { Value = DispatchInfo.DispatchId ?? Guid.Empty },
            new SqlParameter("@DispatchIsolateId", SqlDbType.UniqueIdentifier) { Value = DispatchInfo.DispatchIsolateId ?? Guid.Empty },
            new SqlParameter("@NoOfAliquots", SqlDbType.Int) { Value = DispatchInfo.NoOfAliquots },
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
