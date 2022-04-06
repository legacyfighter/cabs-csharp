using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Pricing;
using LegacyFighter.Cabs.Ride.Details;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace LegacyFighter.Cabs.Ride;

public static class RideSchema
{
  public static void MapUsing(ModelBuilder modelBuilder, ValueConverter<Instant, long> instantConverter)
  {
    modelBuilder.Entity<RequestForTransit>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Ignore(x => x.EstimatedPrice);
      builder.Property(x => x.RequestGuid);
      builder.OwnsOne(x => x.Tariff, MapTariffProperties);
      builder.Property(x => x.Distance)
        .HasColumnName("Km")
        .HasConversion(
          d => d.ToKmInDouble(),
          d => Distance.OfKm(d));
    });
    modelBuilder.Entity<Transit>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Ignore(x => x.Distance);
      builder.Property(x => x.RequestGuid);
      builder.Property(x => x.Status);
      builder.Property("_km").HasColumnName("Km");
      builder.OwnsOne(t => t.Tariff, MapTariffProperties);
    });
    modelBuilder.Entity<TransitDemand>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(x => x.Status);
      builder.Property("_transitRequestGuid").HasColumnName("TransitRequestGuid");
      builder.Property("PickupAddressChangeCounter");
    });
    modelBuilder.Entity<TransitDetails>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(d => d.DateTime).HasConversion(instantConverter);
      builder.Property(d => d.CompleteAt).HasConversion(instantConverter);
      builder.HasOne(d => d.Client);
      builder.Navigation(d => d.Client).AutoInclude();
      builder.Property(d => d.DriverId);
      builder.Property(d => d.CarType).HasConversion<string>();
      builder.HasOne(d => d.From);
      builder.Navigation(d => d.From).AutoInclude();
      builder.HasOne(d => d.To);
      builder.Navigation(d => d.To).AutoInclude();
      builder.Property(d => d.Started).HasConversion(instantConverter);
      builder.Property(d => d.AcceptedAt).HasConversion(instantConverter);
      builder.OwnsOne(d => d.DriversFee,
        navigationBuilder =>
        {
          navigationBuilder.Property(m => m.IntValue).HasColumnName(nameof(TransitDetails.DriversFee));
        });
      builder.OwnsOne(d => d.Price,
        navigationBuilder =>
        {
          navigationBuilder.Property(m => m.IntValue).HasColumnName(nameof(TransitDetails.Price));
        });
      builder.OwnsOne(d => d.EstimatedPrice,
        navigationBuilder =>
        {
          navigationBuilder.Property(m => m.IntValue).HasColumnName(nameof(TransitDetails.EstimatedPrice));
        });
      builder.Property(d => d.Status);
      builder.Property(d => d.RequestGuid);
      builder.Property(d => d.PublishedAt).HasConversion(instantConverter);
      builder.Property(d => d.Distance).HasColumnName("Km")
        .HasConversion(
          distance => distance.ToKmInDouble(),
          value => Distance.OfKm(value));
      builder.Property(d => d.TransitId);
      builder.OwnsOne<Tariff>("Tariff", MapTariffProperties);
    });
  }

  private static void MapTariffProperties<T>(OwnedNavigationBuilder<T, Tariff> navigation) where T : class
  {
    navigation.Property<Money>("_baseFee")
      .HasColumnName(nameof(Tariff.BaseFee))
      .HasConversion(
        money => money.IntValue,
        intValue => new Money(intValue));
    navigation.Property(m => m.KmRate).HasColumnName(nameof(Tariff.KmRate));
    navigation.Property(m => m.Name).HasColumnName(nameof(Tariff.Name));
  }
}