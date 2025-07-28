using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

internal class IsolateMap : IEntityTypeConfiguration<Isolate>
{
    public void Configure(EntityTypeBuilder<Isolate> entity)
    {
        entity.HasKey(e => e.IsolateId);

        entity.ToTable("tblIsolate");

        entity.Property(e => e.IsolateId)
            .ValueGeneratedNever();

        entity.Property(e => e.Comment)
            .HasColumnType("text");

        entity.Property(e => e.CreatedBy)
            .HasMaxLength(20)
            .IsUnicode(false);

        entity.Property(e => e.DateCreated)
            .HasColumnType("datetime");

        entity.Property(e => e.IsolateNomenclature)
            .HasMaxLength(200)
            .IsUnicode(false);

        entity.Property(e => e.LastModified)
            .IsRowVersion()
            .IsConcurrencyToken();

        entity.Property(e => e.Mtalocation)
            .HasMaxLength(500)
            .IsUnicode(false)
            .HasColumnName("MTALocation");

        entity.Property(e => e.PhylogeneticAnalysis)
            .HasColumnType("text");

        entity.Property(e => e.PhylogeneticFileName)
            .HasMaxLength(100)
            .IsUnicode(false);

        entity.Property(e => e.SmsreferenceNumber)
            .HasMaxLength(30)
            .IsUnicode(false)
            .HasColumnName("SMSReferenceNumber");

        entity.Property(e => e.Well)
            .HasMaxLength(10)
            .IsUnicode(false);

        entity.Property(e => e.WhyNotValidToIssue)
            .HasMaxLength(50)
            .IsUnicode(false);
    }
}
