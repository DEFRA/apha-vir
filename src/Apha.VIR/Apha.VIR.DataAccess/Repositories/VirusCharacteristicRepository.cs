using System.Data;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories
{
    public class VirusCharacteristicRepository : RepositoryBase<VirusCharacteristic>, IVirusCharacteristicRepository
    {
        public VirusCharacteristicRepository(VIRDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<VirusCharacteristic>> GetAllVirusCharacteristicsAsync()
        {
            return await GetQueryableInterpolatedFor<VirusCharacteristic>($"EXEC spVirusCharacteristicGetAll").ToListAsync();
        }
        public async Task<PagedData<VirusCharacteristic>> GetAllVirusCharacteristicsAsync(int pageNo, int pageSize)
        {
            var result = await _context.Set<VirusCharacteristic>()
                .FromSqlInterpolated($"EXEC spVirusCharacteristicGetAll").ToListAsync();

            var totalRecords = result.Count;
            var entries = result.Skip((pageNo - 1) * pageSize)
                .Take(pageSize).ToList();

            return new PagedData<VirusCharacteristic>(entries, totalRecords);
        }
        public async Task<VirusCharacteristic?> GetVirusCharacteristicsByIdAsync(Guid id)
        {
            var result = await _context.Set<VirusCharacteristic>()
                .FromSqlInterpolated($"EXEC spVirusCharacteristicGetAll").ToListAsync(); ;
  
            var entry = result.SingleOrDefault(x => x.Id == id);

            return entry;
        }
        public async Task<IEnumerable<VirusCharacteristic>> GetAllVirusCharacteristicsByVirusTypeAsync(Guid? virusType, bool isAbscent)
        {
            if (isAbscent)
            {
                return await GetQueryableInterpolatedFor<VirusCharacteristic>($"EXEC spVirusCharacteristicGetByVirusTypeWhereAbscent @VirusType = {virusType}").ToListAsync();
            }
            else
            {
                return await GetQueryableInterpolatedFor<VirusCharacteristic>($"EXEC spVirusCharacteristicGetByVirusTypeWherePresent @VirusType = {virusType}").ToListAsync();
            }
        }
        public async Task<IEnumerable<VirusCharacteristicDataType>> GetAllVirusCharactersticsTypeNamesAsync()
        {
            return (await _context.Set<VirusCharacteristicDataType>()
            .FromSqlRaw($"EXEC spVirusCharacteristicTypeGetAll").ToListAsync())
            .ToList();
        }
        public async Task AddEntryAsync(VirusCharacteristic virusCharacteristic)
        {
            SqlParameter[] parameters = GetAddSqlParameters(virusCharacteristic);

            await ExecuteSqlAsync(
            @"EXEC spVirusCharacteristicInsert @Id, @Name, @CharacteristicType, @NumericSort, 
                      @DisplayOnSearch, @Prefix, @MinValue, @MaxValue, @DecimalPlaces, @Length, @Index, @LastModified", parameters);
        }

        public async Task UpdateEntryAsync(VirusCharacteristic virusCharacteristic)
        {
            SqlParameter[] parameters = GetUpdateSqlParameters(virusCharacteristic);

            await ExecuteSqlAsync(
            @"EXEC spVirusCharacteristicUpdate @Id, @Name, @CharacteristicType, @NumericSort, 
                      @DisplayOnSearch, @Prefix, @MinValue, @MaxValue, @DecimalPlaces, @Length, @Index, @LastModified", parameters);
        }
        public async Task DeleteVirusCharactersticsAsync(Guid id, byte[] lastModified)
        {
            var parameters = new[]
            {
           new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = id },
           new SqlParameter("@LastModified", SqlDbType.Timestamp) { Value = lastModified },
        };

            await ExecuteSqlAsync(@"EXEC spVirusCharacteristicDelete @Id,@LastModified", parameters);
        }
        public async Task<bool> CheckVirusCharactersticsUsageByIdAsync(Guid id)
        {
            int TotalEntries = 0;
            using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "spVirusCharacteristicGetUsageById";
                    command.CommandType = CommandType.StoredProcedure;

                    var param = command.CreateParameter();
                    param.ParameterName = "@id";
                    param.Value = id;
                    command.Parameters.Add(param);

                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (await result.ReadAsync())
                        {
                            TotalEntries = Convert.ToInt32(result["Entries"].ToString());
                        }
                    }
                }
            }
            return TotalEntries > 0 ? true : false;
        }

        private static SqlParameter[] GetAddSqlParameters(VirusCharacteristic virusCharacteristic)
        {
            return new[]
             {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = virusCharacteristic.Id},
                new SqlParameter("@Name", SqlDbType.VarChar, 50)
                { Value =  virusCharacteristic.Name == null ? DBNull.Value: virusCharacteristic.Name},
                new SqlParameter("@CharacteristicType", SqlDbType.UniqueIdentifier)
                { Value =  virusCharacteristic.CharacteristicType == Guid.Empty ? DBNull.Value: virusCharacteristic.CharacteristicType},
                new SqlParameter("@NumericSort", SqlDbType.Bit) { Value = virusCharacteristic.NumericSort},
                new SqlParameter("@DisplayOnSearch", SqlDbType.Bit) { Value = virusCharacteristic.DisplayOnSearch},
                new SqlParameter("@Prefix", SqlDbType.VarChar, 50) { Value =  virusCharacteristic.Prefix == null ? DBNull.Value: virusCharacteristic.Prefix},
                new SqlParameter("@MinValue", SqlDbType.Float) { Value =  virusCharacteristic.MinValue == null ? DBNull.Value: virusCharacteristic.MinValue},
                new SqlParameter("@MaxValue", SqlDbType.Float) { Value =  virusCharacteristic.MaxValue == null ? DBNull.Value: virusCharacteristic.MaxValue},
                new SqlParameter("@DecimalPlaces", SqlDbType.Int) { Value =  virusCharacteristic.DecimalPlaces == null ? DBNull.Value: virusCharacteristic.DecimalPlaces},
                new SqlParameter("@Length", SqlDbType.Int) { Value =  virusCharacteristic.Length == null ? DBNull.Value: virusCharacteristic.Length},
                new SqlParameter("@Index", SqlDbType.Int) { Value =  virusCharacteristic.CharacteristicIndex == null ? DBNull.Value: virusCharacteristic.CharacteristicIndex},
                new SqlParameter
                {
                    ParameterName = "@LastModified",
                    SqlDbType = SqlDbType.Timestamp,
                    Direction = ParameterDirection.Output
                }
            };
        }

        private static SqlParameter[] GetUpdateSqlParameters(VirusCharacteristic virusCharacteristic)
        {
            return new[]
             {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = virusCharacteristic.Id},
                new SqlParameter("@Name", SqlDbType.VarChar, 50)
                { Value =  virusCharacteristic.Name == null ? DBNull.Value: virusCharacteristic.Name},
                new SqlParameter("@CharacteristicType", SqlDbType.UniqueIdentifier)
                { Value =  virusCharacteristic.CharacteristicType == Guid.Empty ? DBNull.Value: virusCharacteristic.CharacteristicType},
                new SqlParameter("@NumericSort", SqlDbType.Bit) { Value = virusCharacteristic.NumericSort},
                new SqlParameter("@DisplayOnSearch", SqlDbType.Bit) { Value = virusCharacteristic.DisplayOnSearch},
                new SqlParameter("@Prefix", SqlDbType.VarChar, 50) { Value =  virusCharacteristic.Prefix == null ? DBNull.Value: virusCharacteristic.Prefix},
                new SqlParameter("@MinValue", SqlDbType.Float) { Value =  virusCharacteristic.MinValue == null ? DBNull.Value: virusCharacteristic.MinValue},
                new SqlParameter("@MaxValue", SqlDbType.Float) { Value =  virusCharacteristic.MaxValue == null ? DBNull.Value: virusCharacteristic.MaxValue},
                new SqlParameter("@DecimalPlaces", SqlDbType.Int) { Value =  virusCharacteristic.DecimalPlaces == null ? DBNull.Value: virusCharacteristic.DecimalPlaces},
                new SqlParameter("@Length", SqlDbType.Int) { Value =  virusCharacteristic.Length == null ? DBNull.Value: virusCharacteristic.Length},
                new SqlParameter("@Index", SqlDbType.Int) { Value =  virusCharacteristic.CharacteristicIndex == null ? DBNull.Value: virusCharacteristic.CharacteristicIndex},
                new SqlParameter
                {
                    ParameterName = "@LastModified",
                    SqlDbType = SqlDbType.Timestamp,
                    Value =  virusCharacteristic.LastModified == null ? DBNull.Value: virusCharacteristic.LastModified,
                    //Direction = ParameterDirection.Output,
                } 
            };
        }
    }
}
