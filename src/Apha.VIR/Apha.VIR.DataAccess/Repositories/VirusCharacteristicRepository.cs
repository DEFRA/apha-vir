using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Apha.VIR.DataAccess.Repositories
{
    public class VirusCharacteristicRepository : IVirusCharacteristicRepository
    {
        private readonly VIRDbContext _context;

        public VirusCharacteristicRepository(VIRDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<IEnumerable<VirusCharacteristic>> GetAllVirusCharacteristicsAsync()
        {
            var data = await _context.Set<VirusCharacteristic>()
                .FromSqlInterpolated($"EXEC spVirusCharacteristicGetAll").ToListAsync();

            return data;
        }
        public async Task<PagedData<VirusCharacteristic>> GetAllVirusCharacteristicsAsync(int pageNo = 0, int pageSize = 0)
        {
            var result = await _context.Set<VirusCharacteristic>()
                .FromSqlInterpolated($"EXEC spVirusCharacteristicGetAll").ToListAsync();

            var totalRecords = result.Count;
            var entries = result.Skip((pageNo - 1) * pageSize)
                .Take(pageSize).ToList();
            return new PagedData<VirusCharacteristic>(entries, totalRecords);
        }

        public async Task<IEnumerable<VirusCharacteristic>> GetAllVirusCharacteristicsByVirusTypeAsync(Guid? virusType, bool isAbscent)
        {
            if (isAbscent)
            {
                return await _context.Set<VirusCharacteristic>()
               .FromSqlInterpolated($"EXEC spVirusCharacteristicGetByVirusTypeWhereAbscent @VirusType = {virusType}").ToListAsync();
            }
            else
            {
                return await _context.Set<VirusCharacteristic>()
               .FromSqlInterpolated($"EXEC spVirusCharacteristicGetByVirusTypeWherePresent @VirusType = {virusType}").ToListAsync();
            }
        }
        public async Task<IEnumerable<VirusCharacteristicDataType>> GetAllVirusCharactersticsTypeNamesAsync()
        {
            return (await _context.Set<VirusCharacteristicDataType>()
            .FromSqlRaw($"EXEC spVirusCharacteristicTypeGetAll").ToListAsync())
            .ToList();
        }
        public async Task AddEntryAsync(VirusCharacteristic entry)
        {
            var lastModified = new byte[8];
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"EXEC spVirusCharacteristicInsert 
            @Id = {entry.Id}, 
            @Name = {entry.Name}, 
            @CharacteristicType ={entry.CharacteristicType},
            @NumericSort ={entry.NumericSort},
            @DisplayOnSearch ={entry.DisplayOnSearch},
            @Prefix ={entry.Prefix},
            @MinValue ={entry.MinValue},
            @MaxValue ={entry.MaxValue},
            @DecimalPlaces ={entry.DecimalPlaces},
            @Length ={entry.Length},
            @Index ={entry.CharacteristicIndex},
            @LastModified = {lastModified}");
            entry.LastModified = lastModified;
        }
    }
}
