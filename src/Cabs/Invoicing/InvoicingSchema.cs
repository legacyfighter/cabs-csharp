using LegacyFighter.Cabs.Common;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Invoicing;

public class InvoicingSchema
{
  public static void MapUsing(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Invoice>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property("Amount");
      builder.Property("SubjectName");
    });
  }
}