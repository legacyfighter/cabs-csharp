using LegacyFighter.Cabs.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace LegacyFighter.Cabs.Crm.Claims;

public static class ClaimSchema
{
  public static void MapUsing(ModelBuilder modelBuilder, ValueConverter<Instant, long> instantConverter)
  {
    modelBuilder.Entity<Claim>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(c => c.OwnerId);
      builder.Property(c => c.TransitId);
      builder.Property(x => x.ChangeDate).HasConversion(instantConverter);
      builder.Property(x => x.CompletionDate).HasConversion(instantConverter);
      builder.Property(x => x.CreationDate).HasConversion(instantConverter).IsRequired();
      builder.Property(x => x.Reason).IsRequired();
      builder.Property(x => x.IncidentDescription);
      builder.Property(x => x.CompletionMode).HasConversion<string>();
      builder.Property(x => x.Status).HasConversion<string>().IsRequired();
      builder.Property(x => x.ClaimNo).IsRequired();
      builder.OwnsOne(x => x.TransitPrice, navigation =>
      {
        navigation.Property(m => m.IntValue).HasColumnName(nameof(Claim.TransitPrice)).IsRequired();
      });
    });
    modelBuilder.Entity<ClaimAttachment>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.HasOne(a => a.Claim);
      builder.Property(x => x.CreationDate).HasConversion(instantConverter).IsRequired();
      builder.Property(x => x.Data).HasColumnType("BLOB");
      builder.Property(x => x.Description);
    });
    modelBuilder.Entity<ClaimsResolver>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property("ClientId");
      builder.Property("ClaimedTransitsIds");
    });
  }
}