using LegacyFighter.Cabs.Common;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.CarFleet;

public static class CarFleetSchema
{
  public static void MapUsing(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<CarType>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(t => t.CarClass).HasConversion<string>().IsRequired();
      builder.Property(t => t.Description);
      builder.Property(t => t.Status).HasConversion<string>().IsRequired();
      builder.Property(t => t.CarsCounter).IsRequired();
      builder.Property(t => t.MinNoOfCarsToActivateClass).IsRequired();
    });
    modelBuilder.Entity<CarTypeActiveCounter>(builder =>
    {
      builder.HasKey("CarClass");
      builder.Property("CarClass").HasConversion<string>().IsRequired().ValueGeneratedNever();
      builder.Property(t => t.ActiveCarsCounter).IsRequired();
    });
  }
}