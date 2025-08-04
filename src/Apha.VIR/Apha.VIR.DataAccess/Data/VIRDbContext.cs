using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apha.VIR.DataAccess.Data;

public class VIRDbContext : DbContext
{
    public VIRDbContext(DbContextOptions<VIRDbContext> options) : base(options)
    { }

    public virtual DbSet<Characteristic> Characteristics { get; set; }
    public virtual DbSet<Dispatch> Dispatches { get; set; }
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
    public virtual DbSet<VirusTypeCharacteristicMap> VirusTypeCharacteristics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
          modelBuilder.ApplyConfigurationsFromAssembly(typeof(VIRDbContext).Assembly);
    }
}
