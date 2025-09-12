using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class VirusCharacteristicListEntryRepository : RepositoryBase<VirusCharacteristicListEntry>, IVirusCharacteristicListEntryRepository
{

    public VirusCharacteristicListEntryRepository(VIRDbContext context): base(context)
    {
    }

    public async Task<IEnumerable<VirusCharacteristicListEntry>> GetEntriesByCharacteristicIdAsync(Guid virusCharacteristicId)
    {
        return await GetQueryableInterpolatedFor<VirusCharacteristicListEntry>($"EXEC spVirusCharacteristicListEntryGetById @VirusCharacteristicId = {virusCharacteristicId}").ToListAsync();
    }

    public  async Task<PagedData<VirusCharacteristicListEntry>> GetVirusCharacteristicListEntries(Guid virusCharacteristicId, int pageNo, int pageSize)
    {
        var result = await GetQueryableInterpolatedFor<VirusCharacteristicListEntry>($"EXEC spVirusCharacteristicListEntryGetById @VirusCharacteristicId = {virusCharacteristicId}").ToListAsync();

        var totalRecords = result.Count;
        var entries = result.Skip((pageNo - 1) * pageSize)
            .Take(pageSize).ToList();

        return new PagedData<VirusCharacteristicListEntry>(entries, totalRecords);
    }


    public async Task<VirusCharacteristicListEntry?> GetByIdAsync(Guid id)
    {
        return await GetQueryableInterpolatedFor<VirusCharacteristicListEntry>($"SELECT * FROM tlkpVirusCharacteristicListEntry WHERE Id = {id}")
            .FirstOrDefaultAsync();
    }

    public async Task AddEntryAsync(VirusCharacteristicListEntry entry)
    {
        var lastModified = new byte[8];
        await ExecuteSqlInterpolatedAsync(
            $@"EXEC spVirusCharacteristicListEntryInsert 
            @Id = {entry.Id}, 
            @Name = {entry.Name}, 
            @Characteristic = {entry.VirusCharacteristicId}, 
            @LastModified = {lastModified}");
        entry.LastModified = lastModified;
    }

    public async Task UpdateEntryAsync(VirusCharacteristicListEntry entry)
    {
        var lastModified = entry.LastModified ?? new byte[8];
        await ExecuteSqlInterpolatedAsync(
            $@"EXEC spVirusCharacteristicListEntryUpdate 
            @Id = {entry.Id}, 
            @Name = {entry.Name}, 
            @Characteristic = {entry.VirusCharacteristicId}, 
            @LastModified = {lastModified}");
        entry.LastModified = lastModified;
    }

    public async Task DeleteEntryAsync(Guid id, byte[] lastModified)
    {
        await ExecuteSqlInterpolatedAsync(
            $@"EXEC spVirusCharacteristicListEntryDelete 
            @Id = {id}, 
            @LastModified = {lastModified}");
    }
}