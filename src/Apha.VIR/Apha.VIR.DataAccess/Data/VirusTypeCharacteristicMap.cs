using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

public class VirusTypeCharacteristicMap : IEntityTypeConfiguration<VirusTypeCharacteristic>
{
    public void Configure(EntityTypeBuilder<VirusTypeCharacteristic> entity)
    {
        entity.HasKey(e => new { e.VirusCharacteristicId, e.VirusTypeId });

        entity.ToTable("tlnkVirusTypeCharacteristic");

        entity.Property(e => e.SmscolumnId)
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasColumnName("SMSColumnId");
    }
}
