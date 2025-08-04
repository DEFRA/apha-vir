using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

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
            return await _context.Set<VirusCharacteristic>()
                  .FromSqlRaw($"EXEC spVirusCharacteristicGetAll").ToListAsync();
        }

        public async Task<IEnumerable<VirusCharacteristic>> GetAllVirusCharacteristicsByVirusTypeAsync(string? virusType, bool isAbscent)
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
    }
}
