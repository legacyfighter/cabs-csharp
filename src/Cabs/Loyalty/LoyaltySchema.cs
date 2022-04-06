using LegacyFighter.Cabs.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace LegacyFighter.Cabs.Loyalty;

public static class LoyaltySchema
{
  public static void MapUsing(ModelBuilder modelBuilder, ValueConverter<Instant, long> instantConverter)
  {
    modelBuilder.Entity<AwardedMiles>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(m => m.ClientId);
      builder.Property("MilesJson").IsRequired();
      builder.Ignore(m => m.Miles);
      builder.Property(m => m.TransitId);
      builder.HasOne<AwardsAccount>("Account").WithMany("Miles");
      builder.Property(x => x.Date).HasConversion(instantConverter).IsRequired();
      builder.Ignore(x => x.ExpirationDate);
    });
    modelBuilder.Entity<AwardsAccount>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(a => a.ClientId);
      builder.HasMany<AwardedMiles>("Miles").WithOne("Account");
      builder.Property(x => x.Date).HasConversion(instantConverter).IsRequired();
      builder.Property(x => x.Transactions).IsRequired();
      builder.Property(x => x.Active).IsRequired();
    });
  }
}