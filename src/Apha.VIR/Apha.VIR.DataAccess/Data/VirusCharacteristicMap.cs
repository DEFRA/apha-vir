using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

public class VirusCharacteristicMap : IEntityTypeConfiguration<VirusCharacteristic>
{
    public void Configure(EntityTypeBuilder<VirusCharacteristic> entity)
    {
        entity.ToTable("tlkpVirusCharacteristic");

        entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsUnicode(false);

        entity.Property(e => e.CharacteristicType)
            .HasColumnName("CharacteristicType");

        entity.Property(e => e.NumericSort)
            .HasColumnName("NumericSort");

        entity.Property(e => e.DisplayOnSearch)
            .HasColumnName("DisplayOnSearch");

        entity.Property(e => e.Prefix)
            .HasMaxLength(50)
            .IsUnicode(false);

        entity.Property(e => e.MinValue)
            .HasColumnName("MinValue");

        entity.Property(e => e.MaxValue)
            .HasColumnName("MaxValue");

        entity.Property(e => e.DecimalPlaces)
            .HasColumnName("DecimalPlaces");

        entity.Property(e => e.Length)
            .HasColumnName("Length");

        entity.Property(e => e.CharacteristicIndex)
            .HasColumnName("CharacteristicIndex");

        entity.Property(e => e.LastModified)
           .IsRowVersion()
           .IsConcurrencyToken();

        entity.Property(e => e.DataType)
            .HasColumnName("VirusCharacteristicTypeName");
    }
}
