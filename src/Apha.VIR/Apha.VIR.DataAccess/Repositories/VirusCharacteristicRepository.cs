using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories
{
    public class VirusCharacteristicRepository : RepositoryBase<VirusCharacteristic>, IVirusCharacteristicRepository
    {        
        public VirusCharacteristicRepository(VIRDbContext context): base(context)
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
        public async Task<VirusCharacteristic> GetVirusCharacteristicsByIdAsync(Guid? id)
        {
            var result = await _context.Set<VirusCharacteristic>()
                .FromSqlInterpolated($"EXEC spVirusCharacteristicGetAll").ToListAsync();

            var totalRecords = result.Count;
            var entry = result.Where(x => x.Id == id).SingleOrDefault();
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
            var lastModified = new byte[8];
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"EXEC spVirusCharacteristicInsert 
            @Id = {virusCharacteristic.Id}, 
            @Name = {virusCharacteristic.Name}, 
            @CharacteristicType ={virusCharacteristic.CharacteristicType},
            @NumericSort ={virusCharacteristic.NumericSort},
            @DisplayOnSearch ={virusCharacteristic.DisplayOnSearch},
            @Prefix ={virusCharacteristic.Prefix},
            @MinValue ={virusCharacteristic.MinValue},
            @MaxValue ={virusCharacteristic.MaxValue},
            @DecimalPlaces ={virusCharacteristic.DecimalPlaces},
            @Length ={virusCharacteristic.Length},
            @Index ={virusCharacteristic.CharacteristicIndex},
            @LastModified = {lastModified}");
            virusCharacteristic.LastModified = lastModified;
        }
        public async Task UpdateEntryAsync(VirusCharacteristic virusCharacteristic)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"EXEC spVirusCharacteristicUpdate 
            @Id = {virusCharacteristic.Id}, 
            @Name = {virusCharacteristic.Name}, 
            @CharacteristicType ={virusCharacteristic.CharacteristicType},
            @NumericSort ={virusCharacteristic.NumericSort},
            @DisplayOnSearch ={virusCharacteristic.DisplayOnSearch},
            @Prefix ={virusCharacteristic.Prefix},
            @MinValue ={virusCharacteristic.MinValue},
            @MaxValue ={virusCharacteristic.MaxValue},
            @DecimalPlaces ={virusCharacteristic.DecimalPlaces},
            @Length ={virusCharacteristic.Length},
            @Index ={virusCharacteristic.CharacteristicIndex},
            @LastModified = {virusCharacteristic.LastModified}");
        }
    }
}
