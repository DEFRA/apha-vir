using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data
{
    public class VwIsolateMap : IEntityTypeConfiguration<IsolateSearchResult>
    {
        public void Configure(EntityTypeBuilder<IsolateSearchResult> entity)
        {
            entity
                .HasNoKey()
                .ToView("vwIsolate");

            entity.Property(e => e.Avnumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("AVNumber");
            entity.Property(e => e.BreedName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CheckedById).HasColumnName("CheckedByID");
            entity.Property(e => e.Comment).HasColumnType("text");
            entity.Property(e => e.CountryOfOriginName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DateChecked).HasColumnType("datetime");
            entity.Property(e => e.DateCreated).HasColumnType("datetime");
            entity.Property(e => e.Expr1)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.FamilyName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FreezerName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GroupSpeciesName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HostPurposeName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IsoSmsreferenceNumber)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("IsoSMSReferenceNumber");
            entity.Property(e => e.IsolateNomenclature)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.IsolateViabilityIsolateId).HasColumnName("IsolateViabilityIsolateID");
            entity.Property(e => e.IsolationMethodName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastModified)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Mtalocation)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("MTALocation");
            entity.Property(e => e.Nomenclature)
                .HasMaxLength(384)
                .IsUnicode(false);
            entity.Property(e => e.PhylogeneticAnalysis).HasColumnType("text");
            entity.Property(e => e.PhylogeneticFileName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ReceivedDate).HasColumnType("datetime");
            entity.Property(e => e.SampleTypeName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SenderReferenceNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SmsreferenceNumber)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("SMSReferenceNumber");
            entity.Property(e => e.TrayName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TypeName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Well)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.WhyNotValidToIssue)
                .HasMaxLength(50)
                .IsUnicode(false);
        }
    }
}
