using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

public class LookupMap : IEntityTypeConfiguration<Lookup>
{
    public void Configure(EntityTypeBuilder<Lookup> entity)
    {
        entity.ToTable("tlkpLookup");

        entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

        entity.Property(e => e.Name)
               .HasMaxLength(100)
               .IsUnicode(false);

        entity.Property(e => e.Parent)
            .HasColumnName("Parent")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);

        entity.Property(e => e.SelectCommand)
                    .HasMaxLength(100)
                    .IsUnicode(false);

        entity.Property(e => e.InsertCommand)
                    .HasMaxLength(100)
                    .IsUnicode(false);

        entity.Property(e => e.UpdateCommand)
                   .HasMaxLength(100)
                   .IsUnicode(false);

        entity.Property(e => e.DeleteCommand)
                    .HasMaxLength(100)
                    .IsUnicode(false);

        entity.Property(e => e.InUseCommand)
                    .HasMaxLength(100)
                    .IsUnicode(false);

        entity.Property(e => e.ReadOnly)
      .HasColumnName("ReadOnly");

        entity.Property(e => e.Smsrelated)
    .HasColumnName("Smsrelated");

        entity.Property(e => e.LastModified)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}
