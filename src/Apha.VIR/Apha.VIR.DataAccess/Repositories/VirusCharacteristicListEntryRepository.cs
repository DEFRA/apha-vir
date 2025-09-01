using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using Apha.VIR.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Repositories;

public class VirusCharacteristicListEntryRepository : IVirusCharacteristicListEntryRepository
{
    private readonly VIRDbContext _context;

    public VirusCharacteristicListEntryRepository(VIRDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<VirusCharacteristicListEntry>> GetVirusCharacteristicListEntryByVirusCharacteristic(Guid virusCharacteristicId)
    {
        return await _context.Set<VirusCharacteristicListEntry>()
              .FromSqlInterpolated($"EXEC spVirusCharacteristicListEntryGetById @VirusCharacteristicId = {virusCharacteristicId}").ToListAsync();
    }

    public async Task<PagedData<VirusCharacteristicListEntry>> GetVirusCharacteristicListEntries(Guid virusCharacteristicId, int pageNo, int pageSize)
    {
        var result = await _context.Set<VirusCharacteristicListEntry>()
          .FromSqlInterpolated($"EXEC spVirusCharacteristicListEntryGetById @VirusCharacteristicId = {virusCharacteristicId}").ToListAsync();

        var totalRecords = result.Count;
        var entries = result.Skip((pageNo - 1) * pageSize)
            .Take(pageSize).ToList();

        return new PagedData<VirusCharacteristicListEntry>(entries, totalRecords);
    }

    public async Task<VirusCharacteristicListEntry?> GetByIdAsync(Guid id)
    {
        return await _context.Set<VirusCharacteristicListEntry>()
            .FromSqlInterpolated($"SELECT * FROM tlkpVirusCharacteristicListEntry WHERE Id = {id}")
            .FirstOrDefaultAsync();
    }

    public async Task AddEntryAsync(VirusCharacteristicListEntry entry)
    {
        var lastModified = new byte[8];
        await _context.Database.ExecuteSqlInterpolatedAsync(
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
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $@"EXEC spVirusCharacteristicListEntryUpdate 
            @Id = {entry.Id}, 
            @Name = {entry.Name}, 
            @Characteristic = {entry.VirusCharacteristicId}, 
            @LastModified = {lastModified}");
        entry.LastModified = lastModified;
    }

    public async Task DeleteEntryAsync(Guid id, byte[] lastModified)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $@"EXEC spVirusCharacteristicListEntryDelete 
            @Id = {id}, 
            @LastModified = {lastModified}");
    }
}