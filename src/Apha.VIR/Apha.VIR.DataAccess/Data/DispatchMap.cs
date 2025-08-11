using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

public class DispatchMap : IEntityTypeConfiguration<Dispatch>
{
    public void Configure(EntityTypeBuilder<Dispatch> entity)
    {
        entity.HasKey(e => e.DispatchId);

        entity.ToTable("tblDispatch");

        entity.Property(e => e.DispatchId).ValueGeneratedNever();

        entity.Property(e => e.DispatchIsolateId).HasColumnName("DispatchIsolateID");

        entity.Property(e => e.DispatchedDate).HasColumnType("datetime");

        entity.Property(e => e.LastModified)
                    .IsRowVersion()
                    .IsConcurrencyToken();

        entity.Property(e => e.ReasonForDispatch)
                    .HasMaxLength(50)
                    .IsUnicode(false);

        entity.Property(e => e.RecipientAddress)
                    .HasMaxLength(500)
                    .IsUnicode(false);

        entity.Property(e => e.RecipientName)
                    .HasMaxLength(50)
                    .IsUnicode(false);
    }
}
