using LegacyFighter.Cabs.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace LegacyFighter.Cabs.Agreements;

public static class AgreementsSchema
{
  public static void MapUsing(ModelBuilder modelBuilder, ValueConverter<Instant, long> instantConverter)
  {
    modelBuilder.Entity<Contract>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.HasMany("Attachments").WithOne("Contract");
      builder.Property(x => x.CreationDate).HasConversion(instantConverter).IsRequired();
      builder.Property(x => x.AcceptedAt).HasConversion(instantConverter);
      builder.Property(x => x.ChangeDate).HasConversion(instantConverter);
      builder.Property(x => x.RejectedAt).HasConversion(instantConverter);
      builder.Property(x => x.Status).HasConversion<string>().IsRequired();
      builder.Property(x => x.ContractNo).IsRequired();
      builder.Property(x => x.PartnerName);
      builder.Property(x => x.Subject);
    });
    modelBuilder.Entity<ContractAttachment>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(x => x.ContractAttachmentNo).IsRequired();
      builder.Property(x => x.AcceptedAt).HasConversion(instantConverter);
      builder.Property(x => x.ChangeDate).HasConversion(instantConverter);
      builder.Property(x => x.RejectedAt).HasConversion(instantConverter);
      builder.Property(x => x.Status).HasConversion<string>();
      builder.HasOne("Contract").WithMany("Attachments");
    });
    modelBuilder.Entity<ContractAttachmentData>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(x => x.Data).HasColumnType("BLOB");
      builder.Property(x => x.ContractAttachmentNo).IsRequired();
      builder.Property(x => x.CreationDate).HasConversion(instantConverter).IsRequired();
    });
  }
}