using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

public class SystemInfoMap : IEntityTypeConfiguration<SystemInfo>
{
    public void Configure(EntityTypeBuilder<SystemInfo> entity)
    {
        entity.ToTable("tblSysInfo");

        entity.Property(e => e.Id)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("ID");

        entity.Property(e => e.SystemName)
                   .HasMaxLength(50)
                   .IsUnicode(false);

        entity.Property(e => e.DatabaseVersion)
                    .HasMaxLength(20)
                    .IsUnicode(false);

        entity.Property(e => e.ReleaseDate).HasColumnType("datetime");

        entity.Property(e => e.Environment)
                    .HasMaxLength(10)
                    .IsUnicode(false);

        entity.Property(e => e.Live)
           .HasColumnName("Live");

        entity.Property(e => e.ReleaseNotes).HasColumnType("text");
    }
}
