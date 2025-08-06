using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

public class StaffMap : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> entity)
    {
        entity.ToTable("tlkpStaff");

        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.LastModified)
            .IsRowVersion()
            .IsConcurrencyToken();
        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsUnicode(false);
        entity.Property(e => e.Sms).HasColumnName("SMS");
        entity.Property(e => e.Smscode)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("SMSCode");
    }
}