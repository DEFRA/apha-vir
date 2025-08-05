using System.Data;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class IsolateRepository : IIsolateRepository
{
    private readonly VIRDbContext _context;

    public IsolateRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task<IEnumerable<IsolateInfo>> GetIsolateInfoByAVNumberAsync(string AVNumber)
    {
        var isolateInfoList = new List<IsolateInfo>();

        using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "spIsolateGetByAVNumber";
                command.CommandType = CommandType.StoredProcedure;

                var param = command.CreateParameter();
                param.ParameterName = "@AVNumber";
                param.Value = AVNumber;
                command.Parameters.Add(param);

                using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        var dto = new IsolateInfo
                        {
                            AvNumber = result["AVNumber"] as string,
                            Nomenclature = result["Nomenclature"] as string,
                            IsolateId = (Guid)result["IsolateId"]
                        };
                        isolateInfoList.Add(dto);
                    }
                }
            }
        }

        return isolateInfoList;
    }
}
