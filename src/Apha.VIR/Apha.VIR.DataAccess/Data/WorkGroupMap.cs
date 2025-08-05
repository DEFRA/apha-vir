using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

public class WorkGroupMap : IEntityTypeConfiguration<Workgroup>
{
    public void Configure(EntityTypeBuilder<Workgroup> entity)
    {
        entity.ToTable("tlkpWorkgroup");

        entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
        entity.Property(e => e.LastModified)
            .IsRowVersion()
            .IsConcurrencyToken();
        entity.Property(e => e.Name)
            .HasMaxLength(50)
            .IsUnicode(false);
        entity.Property(e => e.Sms).HasColumnName("SMS");
        entity.Property(e => e.Smscode)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("SMSCode");
    }
}