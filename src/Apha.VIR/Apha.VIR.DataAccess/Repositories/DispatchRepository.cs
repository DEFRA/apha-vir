using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
                    while (await result.ReadAsync())
                    {
                        try
                        {
                            var dto = new IsolateDispatchInfo
                            {
                                AVNumber = result["AVNumber"] as string,
                                Nomenclature = result["IsolateNomenclature"] as string,
                                IsolateId = result["IsolateId"] as Guid?,
                                DispatchId = result["DispatchId"] as Guid?,
                                NoOfAliquots = result["NoOfAliquots"] as int?,
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
                        catch (IndexOutOfRangeException)
                        {
                            continue;
                        }
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

    


}
