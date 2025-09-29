using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using Apha.VIR.DataAccess.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories
{
    public class LookupRepository : RepositoryBase<Lookup>, ILookupRepository
    {
        public LookupRepository(VIRDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Lookup>> GetAllLookupsAsync()
        {
            return await GetQueryableInterpolatedFor<Lookup>($"EXEC spLookupGetAll").ToListAsync();
        }

        public async Task<Lookup> GetLookupByIdAsync(Guid lookupId)
        {
            var result = await GetQueryableInterpolatedFor<Lookup>($"EXEC spLookupGetAll").ToListAsync();
            var lookup = result.AsEnumerable().FirstOrDefault(x => x.Id == lookupId);

            return lookup ?? new Lookup();
        }

        public async Task<LookupItem> GetLookupItemAsync(Guid lookupId, Guid lookupItemId)
        {
            Lookup? lookup = await GetDbSetFor<Lookup>().FirstOrDefaultAsync(l => l.Id == lookupId);
            if (lookup != null)
            {
                var result = await GetQueryableInterpolatedFor<LookupItem>($"EXEC {lookup.SelectCommand}").ToListAsync();

                var item = result.AsEnumerable().FirstOrDefault(x => x.Id == lookupItemId);

                return item ?? new LookupItem();
            }
            return new LookupItem();
        }

        public async Task<PagedData<LookupItem>> GetAllLookupItemsAsync(Guid lookupId, int pageNo, int pageSize)
        {
            Lookup? lookup = await GetDbSetFor<Lookup>().FirstOrDefaultAsync(l => l.Id == lookupId);
            if (lookup != null)
            {
                //stored procedure is non-composable SQL and EF does support AsQueryable to get performance of skip. 
                var result = await GetQueryableInterpolatedFor<LookupItem>($"EXEC {lookup.SelectCommand}").ToListAsync();

                var totalRecords = result.Count;
                var lookupitems = result.Skip((pageNo - 1) * pageSize)
                    .Take(pageSize).ToList();

                return new PagedData<LookupItem>(lookupitems, totalRecords);
            }
            return new PagedData<LookupItem>([], 0);
        }

        public async Task<IEnumerable<LookupItem>> GetAllLookupItemsAsync(Guid lookupId)
        {
            Lookup? lookup = await GetDbSetFor<Lookup>().FirstOrDefaultAsync(l => l.Id == lookupId);
            if (lookup != null)
            {
                var result = await GetQueryableInterpolatedFor<LookupItem>($"EXEC {lookup.SelectCommand}").ToListAsync();

                return result;
            }
            return new List<LookupItem>();
        }

        public async Task<IEnumerable<LookupItem>> GetLookupItemParentListAsync(Guid lookupId)
        {
            return await GetAllLookupItemsAsync(lookupId);
        }

        public async Task<bool> IsLookupItemInUseAsync(Guid lookupId, Guid lookupItemId)
        {
            bool result = false;

            Lookup? lookup = await GetDbSetFor<Lookup>().FirstOrDefaultAsync(l => l.Id == lookupId);
            try
            {
                if (lookup != null)
                {
                    var sql = $"EXEC [{lookup.InUseCommand}] @ID";

                    var parameters = new[]
                     {
                        new SqlParameter("@ID", SqlDbType.UniqueIdentifier)
                        { Value = lookupItemId ==Guid.Empty  ? DBNull.Value: lookupItemId}
                    };

                    await ExecuteSqlAsync(sql, parameters);
                }
            }
            catch (Exception ex)
            {
                if (ex is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 547)
                {
                    result = true;
                }
            }

            return result;
        }

        [SuppressMessage("Security", "S3649:SQL queries should not be dynamically built from user input",
         Justification = "Stored procedure name is validated against a whitelist from the database.")]
        public async Task InsertLookupItemAsync(Guid LookupId, LookupItem Item)
        {
            Lookup? lookup = await GetDbSetFor<Lookup>().FirstOrDefaultAsync(l => l.Id == LookupId);

            var allowedProcedures = (await GetDbSetFor<Lookup>().ToListAsync())
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
                new SqlParameter("@ID", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid()},
                new SqlParameter("@Name", SqlDbType.VarChar, 100) { Value =  Item.Name == null ? DBNull.Value: Item.Name},
                new SqlParameter("@AltName", SqlDbType.VarChar, 100) { Value =  Item.AlternateName == null ? DBNull.Value: Item.AlternateName},
                new SqlParameter("@Parent", SqlDbType.UniqueIdentifier) { Value = Item.Parent ==Guid.Empty || Item.Parent == null ? DBNull.Value: Item.Parent},
                new SqlParameter("@Active", SqlDbType.Bit) { Value = Item.Active},
                new SqlParameter  {
                    ParameterName = "@LastModified",
                    SqlDbType = SqlDbType.Timestamp,
                    Direction = ParameterDirection.Output
                }
            };

            await ExecuteSqlAsync(sql, parameters);
        }

        [SuppressMessage("Security", "S3649:SQL queries should not be dynamically built from user input",
         Justification = "Stored procedure name is validated against a whitelist from the database.")]
        public async Task UpdateLookupItemAsync(Guid LookupId, LookupItem Item)
        {
            Lookup? lookup = await GetDbSetFor<Lookup>().FirstOrDefaultAsync(l => l.Id == LookupId);

            var allowedProcedures = (await GetDbSetFor<Lookup>().ToListAsync())
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
                new SqlParameter("@ID", SqlDbType.UniqueIdentifier) { Value = Item.Id ==Guid.Empty  ? DBNull.Value: Item.Id},
                new SqlParameter("@Name", SqlDbType.VarChar, 100) { Value =  Item.Name == null ? DBNull.Value: Item.Name},
                new SqlParameter("@AltName", SqlDbType.VarChar, 100) { Value =  Item.AlternateName == null ? DBNull.Value: Item.AlternateName},
                new SqlParameter("@Parent", SqlDbType.UniqueIdentifier) { Value = Item.Parent ==Guid.Empty || Item.Parent == null ? DBNull.Value: Item.Parent},
                new SqlParameter("@Active", SqlDbType.Bit) { Value = Item.Active},
                new SqlParameter  {
                    ParameterName = "@LastModified",
                    SqlDbType = SqlDbType.Timestamp,
                    Direction = ParameterDirection.InputOutput,
                    Value = Item.LastModified
                }
            };

            await ExecuteSqlAsync(sql, parameters);
        }

        [SuppressMessage("Security", "S3649:SQL queries should not be dynamically built from user input",
         Justification = "Stored procedure name is validated against a whitelist from the database.")]
        public async Task DeleteLookupItemAsync(Guid LookupId, LookupItem Item)
        {

            var allowedProcedures = (await GetDbSetFor<Lookup>().ToListAsync())
                                    .Select(l => l.DeleteCommand)
                                    .Where(cmd => !string.IsNullOrWhiteSpace(cmd))
                                    .Distinct()
                                    .ToList();

            Lookup? lookup = await GetDbSetFor<Lookup>().FirstOrDefaultAsync(l => l.Id == LookupId);

            if (string.IsNullOrWhiteSpace(lookup?.DeleteCommand))
                throw new ArgumentException("Lookup delete Stored procedure name is required.");

            if (!allowedProcedures.Contains(lookup.DeleteCommand))
                throw new SecurityException($"Stored procedure '{lookup.UpdateCommand}' is not allowed.");

            var sql = $"EXEC [{lookup.DeleteCommand}] @ID, @LastModified OUT";

            var parameters = new[]
             {
                new SqlParameter("@ID", SqlDbType.UniqueIdentifier) { Value = Item.Id ==Guid.Empty  ? DBNull.Value: Item.Id},
               new SqlParameter  {
                    ParameterName = "@LastModified",
                    SqlDbType = SqlDbType.Timestamp,
                    Direction = ParameterDirection.InputOutput,
                    Value = Item.LastModified
                }
            };
            await ExecuteSqlAsync(sql, parameters);
        }

        public async Task<IEnumerable<LookupItem>> GetAllVirusFamiliesAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spVirusFamilyGetAll").ToListAsync())
                .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllVirusTypesAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spVirusTypeGetAll").ToListAsync())
                   .Where(vt => vt.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllVirusTypesByParentAsync(Guid? virusFamily)
        {
            return await GetLookupItemsByParentAsync("VirusType", virusFamily);
        }

        public async Task<IEnumerable<LookupItem>> GetAllHostSpeciesAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spHostSpeciesGetAll").ToListAsync())
                .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllHostBreedsAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spHostBreedGetAll").ToListAsync())
             .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllHostBreedsByParentAsync(Guid? hostSpecies)
        {
            return await GetLookupItemsByParentAsync("HostBreed", hostSpecies);
        }

        public async Task<IEnumerable<LookupItem>> GetAllHostBreedsAltNameAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spHostBreedGetAllAltName").ToListAsync())
             .Where(vf => vf.Active).ToList();
        }

        protected virtual async Task<IEnumerable<LookupItem>> GetLookupItemsByParentAsync(string Lookup, Guid? Parent)
        {
            var lookupItemList = new List<LookupItem>();
            string commandName = string.Empty;
            if (Lookup == "HostBreed")
            {
                commandName = "spHostBreedGetByParent";
            }
            else if (Lookup == "VirusType")
            {
                commandName = "spVirusTypeGetByParent";
            }
            else if (Lookup == "Tray")
            {
                commandName = "spTrayGetByParent";
            }

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
            return (await GetQueryableResultFor<LookupItem>($"EXEC spCountryGetAll").ToListAsync())
             .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllHostPurposesAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spHostPurposeGetAll").ToListAsync())
               .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllSampleTypesAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spSampleTypeGetAll").ToListAsync())
                .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllWorkGroupsAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spWorkgroupsGetAll").ToListAsync())
            .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllStaffAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spStaffGetAll").ToListAsync())
            .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllViabilityAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spViabilityGetAll").ToListAsync())
            .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllSubmittingLabAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spSubmittingLabGetAll").ToListAsync())
            .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllSubmissionReasonAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spSubmissionReasonGetAll").ToListAsync())
            .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllIsolationMethodsAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spIsolationMethodGetAll").ToListAsync())
            .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllFreezerAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spFreezerGetAll").ToListAsync())
            .Where(vf => vf.Active).ToList();
        }

        public async Task<IEnumerable<LookupItem>> GetAllTraysAsync()
        {
            return (await GetQueryableResultFor<LookupItem>($"EXEC spTrayGetAll").ToListAsync())
            .Where(vf => vf.Active).ToList();
        }
       

        public async Task<IEnumerable<LookupItem>> GetAllTraysByParentAsync(Guid? freezer)
        {
            return await GetLookupItemsByParentAsync("Tray", freezer);
        }
    }
}
