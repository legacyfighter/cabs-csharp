using LegacyFighter.Cabs.Parties.Model.Parties;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Parties;

public static class PartiesSchema
{
  public static void MapUsing(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Party>(builder =>
    {
      builder.HasKey(p => p.Id);
      builder.Property(p => p.Id).ValueGeneratedNever();
    });
    modelBuilder.Entity<PartyRelationship>(builder =>
    {
      builder.HasKey(p => p.Id);
      builder.Property(p => p.Name);
      builder.Property(p => p.RoleA);
      builder.Property(p => p.RoleB);
      builder.HasOne(p => p.PartyA);
      builder.HasOne(p => p.PartyB);
    });
    modelBuilder.Entity<PartyRole>(builder =>
    {
      builder.HasKey(p => p.Id);
      builder.Property(p => p.Name);
    });
  }
}