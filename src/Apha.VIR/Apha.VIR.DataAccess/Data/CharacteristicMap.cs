using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

internal class CharacteristicMap : IEntityTypeConfiguration<Characteristic>
{
    public void Configure(EntityTypeBuilder<Characteristic> entity)
    {
        entity.HasKey(e => e.CharacteristicId);

        entity.ToTable("tblCharacteristic");

        entity.Property(e => e.CharacteristicId).ValueGeneratedNever();

        entity.Property(e => e.CharacteristicValue)
            .HasMaxLength(50)
            .IsUnicode(false);

        entity.Property(e => e.LastModified)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}
