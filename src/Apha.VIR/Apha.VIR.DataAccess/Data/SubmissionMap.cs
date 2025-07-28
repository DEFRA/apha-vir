using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

public class SubmissionMap : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> entity)
    {
        entity.HasKey(e => e.SubmissionId);

        entity.ToTable("tblSubmission");

        entity.HasIndex(e => e.Avnumber, "UQ__tblSubmission__7F60ED59").IsUnique();

        entity.Property(e => e.SubmissionId).ValueGeneratedNever();

        entity.Property(e => e.Avnumber)
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasColumnName("AVNumber");

        entity.Property(e => e.Cphnumber)
            .HasMaxLength(14)
            .IsUnicode(false)
            .HasColumnName("CPHNumber");

        entity.Property(e => e.DateSubmissionReceived)
            .HasColumnType("datetime");

        entity.Property(e => e.LastModified)
            .IsRowVersion()
            .IsConcurrencyToken();

        entity.Property(e => e.Owner)
            .HasMaxLength(50)
            .IsUnicode(false);

        entity.Property(e => e.RlreferenceNumber)
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasColumnName("RLReferenceNumber");

        entity.Property(e => e.SamplingLocationPremises)
            .HasMaxLength(500)
            .IsUnicode(false);

        entity.Property(e => e.Sender)
            .HasMaxLength(50)
            .IsUnicode(false);

        entity.Property(e => e.SenderAddress)
            .HasMaxLength(500)
            .IsUnicode(false);

        entity.Property(e => e.SenderOrganisation)
            .HasMaxLength(200)
            .IsUnicode(false);

        entity.Property(e => e.SendersReferenceNumber)
            .HasMaxLength(50)
            .IsUnicode(false);
    }
}