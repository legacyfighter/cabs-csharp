using LegacyFighter.Cabs.Common;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Crm;

public static class CrmSchema
{
  public static void MapUsing(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Client>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(c => c.ClientType).HasConversion<string>();
      builder.Property(c => c.DefaultPaymentType).HasConversion<string>();
    });
  }
}