using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

public class IsolateViabilityMap : IEntityTypeConfiguration<IsolateViability>
{
    public void Configure(EntityTypeBuilder<IsolateViability> entity)
    {
        entity.HasKey(e => e.IsolateViabilityId)
            .HasName("PK_tblViability");

        entity.ToTable("tblIsolateViability");

        entity.Property(e => e.IsolateViabilityId)
            .HasDefaultValueSql("(newid())");

        entity.Property(e => e.IsolateViabilityIsolateId)
            .HasColumnName("IsolateViabilityIsolateID");

        entity.Property(e => e.Viable)
            .HasColumnName("Viable");

        entity.Property(e => e.DateChecked)
            .HasColumnType("datetime");

        entity.Property(e => e.CheckedById)
            .HasColumnName("CheckedByID");
       
       
        entity.Property(e => e.LastModified)
                    .IsRowVersion()
                    .IsConcurrencyToken();
    }
}
