using System.Data;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class CharacteristicRepository : ICharacteristicRepository
{
    private readonly VIRDbContext _context;

    public CharacteristicRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<IsolateCharacteristicInfo>> GetIsolateCharacteristicInfoAsync(Guid isolateId)
    {
        return await GetIsolateCharacteristics(isolateId);
    }

    private async Task<IEnumerable<IsolateCharacteristicInfo>> GetIsolateCharacteristics(Guid isolateId)
    {
        var isolateCharacteristicList = new List<IsolateCharacteristicInfo>();

        using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "spCharacteristicGetByIsolate";
                command.CommandType = CommandType.StoredProcedure;

                var param = command.CreateParameter();
                param.ParameterName = "@IsolateID";
                param.Value = isolateId;
                command.Parameters.Add(param);

                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {

                        var dto = new IsolateCharacteristicInfo
                        {
                            CharacteristicId = (Guid)result["CharacteristicId"],
                            CharacteristicValue = result["CharacteristicValue"] as string,
                            CharacteristicIsolateId = (Guid)result["CharacteristicIsolateId"],
                            CharacteristicPrefix = result["CharacteristicPrefix"] as string,
                            CharacteristicDisplay = result["CharacteristicDisplay"] as bool?,
                            CharacteristicName = result["CharacteristicName"] as string,
                            CharacteristicType  = result["CharacteristicType"] as string,
                            VirusCharacteristicId = (Guid)result["VirusCharacteristicId"],
                            LastModified = (Byte[])result["LastModified"]
                        };
                        isolateCharacteristicList.Add(dto);

                    }
                }
            }
        }

        return isolateCharacteristicList;
    }

    public async Task UpdateIsolateCharacteristicsAsync(IsolateCharacteristicInfo item, string User)
    {
        await _context.Database.ExecuteSqlRawAsync(
           "EXEC spCharacteristicUpdate @UserID, @CharacteristicId, @CharacteristicIsolateId, @VirusCharacteristicId, @CharacteristicValue, @LastModified OUTPUT",
                new SqlParameter("@UserId", SqlDbType.VarChar, 20) { Value = User },
                new SqlParameter("@CharacteristicId", SqlDbType.UniqueIdentifier) { Value = item.CharacteristicId },
                new SqlParameter("@CharacteristicIsolateId", SqlDbType.UniqueIdentifier) { Value = item.CharacteristicIsolateId },
                new SqlParameter("@VirusCharacteristicId", SqlDbType.UniqueIdentifier) { Value = item.VirusCharacteristicId },
                new SqlParameter("@CharacteristicValue", SqlDbType.VarChar, 30) { Value = (object?)item.CharacteristicValue ?? DBNull.Value },
                new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = (object?)item.LastModified ?? DBNull.Value, Direction = ParameterDirection.InputOutput }
           );
    }
}