using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LegacyFighter.Cabs.Common;

public static class EfCoreExtensions
{
  public static void MapBaseEntityProperties<T>(this EntityTypeBuilder<T> builder) where T : BaseEntity
  {
    builder.HasKey(e => e.Id);
    builder.Property("Version").IsRowVersion();
  }
}