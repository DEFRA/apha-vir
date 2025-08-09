using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data
{
    internal class LookupItemByParentMap : IEntityTypeConfiguration<LookupItemByParent>
    {
        public void Configure(EntityTypeBuilder<LookupItemByParent> entity)
        {
            entity.Property(e => e.Name)
                         .HasMaxLength(100)
                         .IsUnicode(false)
                         .HasColumnName("Name");

            entity.Property(e => e.AlternateName)
                   .HasMaxLength(100)
                   .IsUnicode(false)
                   .HasColumnName("AltName");

            entity.Property(e => e.Active)
                .HasDefaultValue(true)
                .HasColumnName("Active");

            entity.Property(e => e.Sms)
                        .HasDefaultValue(true)
                        .HasColumnName("SMS");

            entity.Property(e => e.Smscode)
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnName("SMSCode");
        }
    }
}
