using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
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
    }
}
