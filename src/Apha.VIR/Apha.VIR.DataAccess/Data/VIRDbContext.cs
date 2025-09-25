using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Data;

public class VIRDbContext : DbContext
{
    public VIRDbContext()
    { }
    public VIRDbContext(DbContextOptions<VIRDbContext> options) : base(options)
    { }

    public virtual DbSet<Isolate> Isolates { get; set; }
    public virtual DbSet<IsolateViability> IsolateViabilities { get; set; }
    public virtual DbSet<Lookup> Lookups { get; set; }
    public virtual DbSet<Sample> Samples { get; set; }
    public virtual DbSet<Sender> Senders { get; set; }
    public virtual DbSet<Submission> Submissions { get; set; }
    public virtual DbSet<SystemInfo> SystemInfos { get; set; }
    public virtual DbSet<VirusCharacteristicDataType> VirusCharacteristicDataTypes { get; set; }
    public virtual DbSet<VirusCharacteristicListEntry> VirusCharacteristicListEntries { get; set; }
    public virtual DbSet<VirusCharacteristic> VirusCharacteristics { get; set; }   
    public virtual DbSet<IsolateSearchResult> VwIsolates { get; set; }
    public virtual DbSet<IsolateCharacteristicsForSearch> VwCharacteristicsForSearches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VIRDbContext).Assembly);
        modelBuilder.Entity<IsolateInfo>().HasNoKey();
        modelBuilder.Entity<IsolateDispatchInfo>().HasNoKey();        
        modelBuilder.Entity<AuditCharacteristicLog>().HasNoKey();
        modelBuilder.Entity<AuditDispatchLog>().HasNoKey();
        modelBuilder.Entity<AuditIsolateLog>().HasNoKey();
        modelBuilder.Entity<AuditSampleLog>().HasNoKey();
        modelBuilder.Entity<AuditSubmissionLog>().HasNoKey();
        modelBuilder.Entity<AuditViabilityLog>().HasNoKey();
        modelBuilder.Entity<AuditIsolateLogDetail>().HasNoKey();
    }
}
