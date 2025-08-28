using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories
{
    public class LookupRepository : ILookupRepository
    {
        private readonly VIRDbContext _context;

        public LookupRepository(VIRDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Lookup>> GetAllLookupsAsync()
        {
            return await _context.Lookups.FromSqlInterpolated($"EXEC spHostBreedGetAll").ToListAsync();
        }

        public async Task<IEnumerable<LookupItem>> GetAllLookupEntriesAsync(Guid LookupId)
        {
            Lookup? lookup = await _context.Lookups.Where(l => l.Id == LookupId).FirstOrDefaultAsync();
            if (lookup != null)
            {
                return await _context.Set<LookupItem>()
                   .FromSqlInterpolated($"EXEC {lookup.SelectCommand}").ToListAsync();
            }
            throw new NotImplementedException();
        }

        [SuppressMessage("Security", "S3649:SQL queries should not be dynamically built from user input",
         Justification = "Stored procedure name is validated against a whitelist from the database.")]
        public async Task InsertLookupEntryAsync(Guid LookupId, LookupItem Item)
        {
            Lookup? lookup = await _context.Lookups.Where(l => l.Id == LookupId).FirstOrDefaultAsync();

            var allowedProcedures = (await _context.Lookups.ToListAsync())
                                .Select(l => l.InsertCommand)
                                .Where(cmd => !string.IsNullOrWhiteSpace(cmd))
                                .Distinct()
                                .ToList();

            if (string.IsNullOrWhiteSpace(lookup?.InsertCommand))
                throw new ArgumentException("Lookup Insert Stored procedure name is required.");

            if (!allowedProcedures.Contains(lookup.InsertCommand))
                throw new SecurityException($"Stored procedure '{lookup.InsertCommand}' is not allowed.");


            var sql = $"EXEC [{lookup.InsertCommand}] @ID, @Name, @AltName, @Parent, @Active, @LastModified OUT";

            var parameters = new[]
            {
                new SqlParameter("@ID", Item.Id),
                new SqlParameter("@Name", Item.Name),
                new SqlParameter("@AltName", Item.AlternateName),
                new SqlParameter("@Parent", Item.ParentName),
                new SqlParameter("@Active", Item.Active),
                new SqlParameter  {
                    ParameterName = "@LastModified",
                    SqlDbType = SqlDbType.Timestamp,
                    Direction = ParameterDirection.Output
                }
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        [SuppressMessage("Security", "S3649:SQL queries should not be dynamically built from user input",
         Justification = "Stored procedure name is validated against a whitelist from the database.")]
        public async Task UpdateLookupEntryAsync(Guid LookupId, LookupItem Item)
        {
            Lookup? lookup = await _context.Lookups.Where(l => l.Id == LookupId).FirstOrDefaultAsync();

            var allowedProcedures = (await _context.Lookups.ToListAsync())
                                .Select(l => l.UpdateCommand)
                                .Where(cmd => !string.IsNullOrWhiteSpace(cmd))
                                .Distinct()
                                .ToList();

            if (string.IsNullOrWhiteSpace(lookup?.UpdateCommand))
                throw new ArgumentException("Lookup update Stored procedure name is required.");

            if (!allowedProcedures.Contains(lookup.UpdateCommand))
                throw new SecurityException($"Stored procedure '{lookup.UpdateCommand}' is not allowed.");

            var sql = $"EXEC [{lookup.UpdateCommand}] @ID, @Name, @AltName, @Parent, @Active, @LastModified OUT";

            var parameters = new[]
             {
                new SqlParameter("@ID", Item.Id),
                new SqlParameter("@Name", Item.Name),
                new SqlParameter("@AltName", Item.AlternateName),
                new SqlParameter("@Parent", Item.ParentName),
                new SqlParameter("@Active", Item.Active),
                new SqlParameter  {
                    ParameterName = "@LastModified",
                    SqlDbType = SqlDbType.Timestamp,
                    Direction = ParameterDirection.Output
                }
            };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        [SuppressMessage("Security", "S3649:SQL queries should not be dynamically built from user input",
         Justification = "Stored procedure name is validated against a whitelist from the database.")]
        public async Task DeleteLookupEntryAsync(Guid LookupId, LookupItem Item)
        {

            var allowedProcedures = (await _context.Lookups.ToListAsync())
                                    .Select(l => l.DeleteCommand)
                                    .Where(cmd => !string.IsNullOrWhiteSpace(cmd))
                                    .Distinct()
                                    .ToList();

            Lookup? lookup = await _context.Lookups.Where(l => l.Id == LookupId).FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(lookup?.DeleteCommand))
                throw new ArgumentException("Lookup delete Stored procedure name is required.");

            if (!allowedProcedures.Contains(lookup.DeleteCommand))
                throw new SecurityException($"Stored procedure '{lookup.UpdateCommand}' is not allowed.");


            var sql = $"EXEC [{lookup.DeleteCommand}] @ID, @LastModified OUT";

            var parameters = new[]
             {
                new SqlParameter("@ID", Item.Id),
                new SqlParameter  {
                    ParameterName = "@LastModified",
                    SqlDbType = SqlDbType.Timestamp,
                    Direction = ParameterDirection.Output
                }
            };
            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public async Task<IEnumerable<LookupItem>> GetAllVirusFamiliesAsync()
        {
            return (await _context.Set<LookupItem>()
                .FromSqlRaw($"EXEC spVirusFamilyGetAll").ToListAsync())
                .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllVirusTypesAsync()
        {
            return (await _context.Set<LookupItem>()
                   .FromSqlRaw($"EXEC spVirusTypeGetAll").ToListAsync())
                   .Where(vt => vt.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllVirusTypesByParentAsync(Guid? virusFamily)
        {
            return await GetLookupItemsByParentAsync("VirusType", virusFamily);
        }

        public async Task<IEnumerable<LookupItem>> GetAllHostSpeciesAsync()
        {
            return (await _context.Set<LookupItem>()
                .FromSqlRaw($"EXEC spHostSpeciesGetAll").ToListAsync())
                .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllHostBreedsAsync()
        {
            return (await _context.Set<LookupItem>()
             .FromSqlRaw($"EXEC spHostBreedGetAll").ToListAsync())
             .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllHostBreedsByParentAsync(Guid? hostSpecies)
        {
            return await GetLookupItemsByParentAsync("HostBreed", hostSpecies);
        }

        public async Task<IEnumerable<LookupItem>> GetAllHostBreedsAltNameAsync()
        {
            return (await _context.Set<LookupItem>()
             .FromSqlRaw($"EXEC spHostBreedGetAllAltName").ToListAsync())
             .Where(vf => vf.Active).ToList();
        }

        private async Task<IEnumerable<LookupItem>> GetLookupItemsByParentAsync(string Lookup, Guid? Parent)
        {
            var lookupItemList = new List<LookupItem>();
            string commandName = string.Empty;
            if (Lookup == "HostBreed")
                commandName = "spHostBreedGetByParent";
            else if (Lookup == "VirusType")
                commandName = "spVirusTypeGetByParent";

            using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandName;
                    command.CommandType = CommandType.StoredProcedure;

                    var param = command.CreateParameter();
                    param.ParameterName = "@Parent";
                    param.Value = Parent;
                    command.Parameters.Add(param);

                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (await result.ReadAsync())
                        {
                            if ((bool)result["Active"])
                            {
                                var dto = new LookupItem
                                {
                                    Id = (Guid)result["Id"],
                                    Name = (string)result["Name"],
                                    AlternateName = result["AltName"] as string,
                                    Active = (bool)result["Active"],
                                    Sms = (bool)result["SMS"],
                                    Smscode = result["SMSCode"] as string
                                };
                                lookupItemList.Add(dto);
                            }
                        }
                    }
                }
            }
            return lookupItemList;
        }

        public async Task<IEnumerable<LookupItem>> GetAllCountriesAsync()
        {
            return (await _context.Set<LookupItem>()
             .FromSqlRaw($"EXEC spCountryGetAll").ToListAsync())
             .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllHostPurposesAsync()
        {
            return (await _context.Set<LookupItem>()
               .FromSqlRaw($"EXEC spHostPurposeGetAll").ToListAsync())
               .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllSampleTypesAsync()
        {
            return (await _context.Set<LookupItem>()
                .FromSqlRaw($"EXEC spSampleTypeGetAll").ToListAsync())
                .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllWorkGroupsAsync()
        {
            return (await _context.Set<LookupItem>()
            .FromSqlRaw($"EXEC spWorkgroupsGetAll").ToListAsync())
            .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllStaffAsync()
        {
            return (await _context.Set<LookupItem>()
            .FromSqlRaw($"EXEC spStaffGetAll").ToListAsync())
            .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllViabilityAsync()
        {
            return (await _context.Set<LookupItem>()
            .FromSqlRaw($"EXEC spViabilityGetAll").ToListAsync())
            .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllSubmittingLabAsync()
        {
            return (await _context.Set<LookupItem>()
            .FromSqlRaw($"EXEC spSubmittingLabGetAll").ToListAsync())
            .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllSubmissionReasonAsync()
        {
            return (await _context.Set<LookupItem>()
            .FromSqlRaw($"EXEC spSubmissionReasonGetAll").ToListAsync())
            .Where(vf => vf.Active).ToList();
        }
    }
}
