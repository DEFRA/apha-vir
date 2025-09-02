using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

public class SampleMap : IEntityTypeConfiguration<Sample>
{
    public void Configure(EntityTypeBuilder<Sample> entity)
    {
        entity.HasKey(e => e.SampleId);

        entity.ToTable("tblSample");

        entity.Property(e => e.SampleId).ValueGeneratedNever();

        entity.Property(e => e.LastModified)
            .IsRowVersion()
            .IsConcurrencyToken();

        entity.Property(e => e.SamplingLocationHouse)
            .HasMaxLength(50)
            .IsUnicode(false);

        entity.Property(e => e.SenderReferenceNumber)
            .HasMaxLength(50)
            .IsUnicode(false);

        entity.Property(e => e.SMSReferenceNumber)
            .HasMaxLength(30)
            .IsUnicode(false);
            
    }
}