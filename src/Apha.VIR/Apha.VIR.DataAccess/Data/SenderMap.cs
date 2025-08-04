using Apha.VIR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.VIR.DataAccess.Data;

public class SenderMap : IEntityTypeConfiguration<Sender>
{
    public void Configure(EntityTypeBuilder<Sender> entity)
    {
        entity.HasKey(e => e.SenderId);

        entity.ToTable("tlkpSender");

        entity.Property(e => e.SenderId)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("SenderID");

        entity.Property(e => e.SenderName)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("Sender"); 

        entity.Property(e => e.SenderAddress)
            .HasMaxLength(500)
            .IsUnicode(false);

        entity.Property(e => e.SenderOrganisation)
            .HasMaxLength(200)
            .IsUnicode(false);
    }
}
