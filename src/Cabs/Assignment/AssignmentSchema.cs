using LegacyFighter.Cabs.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace LegacyFighter.Cabs.Assignment;

public static class AssignmentSchema
{
  public static void MapUsing(ModelBuilder modelBuilder, ValueConverter<Instant, long> instantConverter)
  {
    modelBuilder.Entity<DriverAssignment>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property("_publishedAt")
        .HasColumnName("PublishedAt")
        .HasConversion(instantConverter);
      builder.Property("_driversRejections")
        .HasColumnName("DriversRejections");
      builder.Property("_proposedDrivers")
        .HasColumnName("ProposedDrivers");
      builder.Property(x => x.RequestGuid);
      builder.Property(x => x.Status);
      builder.Property(x => x.AssignedDriver);
      builder.Property(x => x.AwaitingDriversResponses);
    });
  }
}