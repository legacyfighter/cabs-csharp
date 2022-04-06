using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.DriverFleet.DriverReports.TravelledDistances;
using LegacyFighter.Cabs.Geolocation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace LegacyFighter.Cabs.DriverFleet;

public static class DriverFleetSchema
{
  public static void MapUsing(ModelBuilder modelBuilder, ValueConverter<Instant, long> instantConverter)
  {
    modelBuilder.Entity<Driver>(e =>
    {
      e.MapBaseEntityProperties();
      e.Property(d => d.Status).HasConversion<string>().IsRequired();
      e.Property(d => d.Type).HasConversion<string>();
      e.OwnsOne(driver => driver.DriverLicense,
        builder =>
        {
          builder.Property(dl => dl.ValueAsString).HasColumnName(nameof(Driver.DriverLicense)).IsRequired();
        });
      e.HasMany(d => d.Attributes).WithOne(d => d.Driver);
      e.HasOne(d => d.Fee).WithOne(f => f.Driver).HasForeignKey<DriverFee>(x => x.Id);
    });
    modelBuilder.Entity<DriverAttribute>(builder =>
    {
      builder.HasKey("Id");
      builder.Property(a => a.Name).HasConversion<string>().IsRequired();
      builder.Property(a => a.Value).IsRequired();
      builder.HasOne(a => a.Driver).WithMany(d => d.Attributes);
    });
    modelBuilder.Entity<DriverFee>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(f => f.FeeType).IsRequired();
      builder.Property(f => f.Amount).IsRequired();
      builder.OwnsOne(f => f.Min,
        navigation => { navigation.Property(m => m.IntValue).HasColumnName(nameof(DriverFee.Min)); });
    });
    modelBuilder.Entity<TravelledDistance>(builder =>
    {
      builder.HasKey("IntervalId");
      builder.Property<long>("_driverId").HasColumnName("DriverId").IsRequired();
      builder.Property(e => e.LastLatitude).IsRequired();
      builder.Property(e => e.LastLongitude).IsRequired();
      builder.OwnsOne<TimeSlot>("TimeSlot", navigation =>
      {
        navigation.Property(s => s.Beginning).HasColumnName("Beginning").HasConversion(instantConverter);
        navigation.Property(s => s.End).HasColumnName("End").HasConversion(instantConverter);
      });
      builder.Property<Distance>("Distance").HasColumnName("Km")
        .HasConversion(
          distance => distance.ToKmInDouble(),
          value => Distance.OfKm(value)).IsRequired();
    });
  }
}