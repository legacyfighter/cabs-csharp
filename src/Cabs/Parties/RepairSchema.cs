using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Repair.Legacy.Parts;
using LegacyFighter.Cabs.Repair.Legacy.User;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Parties;

public static class RepairSchema
{
  public static void MapUsing(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<EmployeeDriverWithOwnCar>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.HasOne<SignedContract>(nameof(EmployeeDriverWithOwnCar.Contract));
    });
    modelBuilder.Entity<SignedContract>(builder =>
    {
      builder.Property(c => c.CoverageRatio);
      builder.Property(c => c.CoveredParts)
        .HasConversion( //https://github.com/dotnet/efcore/issues/4179#issuecomment-447993816
          parts => string.Join(",", parts.Select(p => p.ToString())),
          str => str.Split(",", StringSplitOptions.None).Select(Enum.Parse<Part>).ToHashSet());
    });
  }
}