using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

public class VirusCharacteristicListEntryMap : IEntityTypeConfiguration<VirusCharacteristicListEntry>
{
    public void Configure(EntityTypeBuilder<VirusCharacteristicListEntry> entity)
    {
        entity.ToTable("tlkpVirusCharacteristicListEntry");

        entity.Property(e => e.Id).ValueGeneratedNever();

        entity.Property(e => e.VirusCharacteristicId)
          .HasColumnName("VirusCharacteristicId");

        entity.Property(e => e.Name)
            .HasMaxLength(50)
            .IsUnicode(false);

        entity.Property(e => e.LastModified)
           .IsRowVersion()
           .IsConcurrencyToken();
    }
}
