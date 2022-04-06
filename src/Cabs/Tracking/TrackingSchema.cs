using LegacyFighter.Cabs.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace LegacyFighter.Cabs.Tracking;

static internal class TrackingSchema
{
  public static void MapUsing(ModelBuilder modelBuilder, ValueConverter<Instant, long> instantConverter)
  {
    modelBuilder.Entity<DriverPosition>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(p => p.DriverId);
      builder.Property(p => p.Latitude).IsRequired();
      builder.Property(p => p.Longitude).IsRequired();
      builder.Property(p => p.SeenAt).HasConversion(instantConverter).IsRequired();
    });
    modelBuilder.Entity<DriverSession>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(x => x.LoggedAt).HasConversion(instantConverter).IsRequired();
      builder.Property(x => x.LoggedOutAt).HasConversion(instantConverter);
      builder.Property(x => x.PlatesNumber).IsRequired();
      builder.Property(x => x.CarClass).HasConversion<string>();
      builder.Property(s => s.CarBrand);
      builder.Property(s => s.DriverId);
    });
  }
}