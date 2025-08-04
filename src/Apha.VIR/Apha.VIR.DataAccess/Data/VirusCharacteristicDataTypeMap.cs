using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

public class VirusCharacteristicDataTypeMap : IEntityTypeConfiguration<VirusCharacteristicDataType>
{
    public void Configure(EntityTypeBuilder<VirusCharacteristicDataType> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK_tlkpVirusCharacteristicType");

        entity.ToTable("tlkpVirusCharacteristicDataType");

        entity.Property(e => e.Id)
            .HasDefaultValueSql("(newid())");

        entity.Property(e => e.DataType)
            .HasMaxLength(50)
            .IsUnicode(false);
    }
}
