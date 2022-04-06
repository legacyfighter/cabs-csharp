using LegacyFighter.Cabs.Common;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Geolocation;

public static class GeolocationSchema
{
  public static void MapUsing(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Address.Address>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.HasIndex(u => u.Hash).IsUnique();
    });
  }
}