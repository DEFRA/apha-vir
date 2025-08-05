using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data
{
    public class VwCharacteristicsForSearchMap : IEntityTypeConfiguration<IsolateCharacteristicsForSearch>
    {
        public void Configure(EntityTypeBuilder<IsolateCharacteristicsForSearch> entity)
        {
            entity
                .HasNoKey()
                .ToView("vwCharacteristicsForSearch");

            entity.Property(e => e.CharacteristicValue)
                .HasMaxLength(50)
                .IsUnicode(false);
        }
    }
}
