using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Contracts.Legacy;
using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Contracts.Model.Content;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Contracts;

public static class ContractsSchema
{
  public static void MapUsing(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Document>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.HasMany<User>("AssignedUsers").WithMany("AssignedDocuments");
      builder.HasOne<User>("Creator").WithMany("CreatedDocuments");
      builder.HasOne<User>("Verifier").WithMany("VerifiedDocuments");
      builder.Property("Content");
      builder.Property("Number");
      builder.Property(x => x.Status).HasConversion<string>();
    });
    modelBuilder.Entity<User>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.HasMany<Document>("AssignedDocuments").WithMany("AssignedUsers");
      builder.HasMany<Document>("CreatedDocuments").WithOne("Creator");
      builder.HasMany<Document>("VerifiedDocuments").WithOne("Verifier");
    });
    modelBuilder.Entity<DocumentContent>(builder =>
    {
      builder.HasKey(c => c.Id);
      builder.Property("PreviousId");
      builder.Property(c => c.PhysicalContent);
      builder.OwnsOne(c => c.DocumentVersion, navigationBuilder =>
      {
        navigationBuilder.Property("_contentVersion")
          .HasColumnName(nameof(DocumentContent.DocumentVersion))
          .UsePropertyAccessMode(PropertyAccessMode.Field);
      });
    });
    modelBuilder.Entity<DocumentHeader>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.OwnsOne(h => h.DocumentNumber, navigationBuilder =>
      {
        navigationBuilder.Property("_number")
          .HasColumnName(nameof(DocumentHeader.DocumentNumber))
          .UsePropertyAccessMode(PropertyAccessMode.Field);
      });
      builder.Property("VerifierId");
      builder.Property(h => h.AuthorId);
      builder.Property(h => h.StateDescriptor);
      builder.OwnsOne(h => h.ContentId, navigationBuilder =>
      {
        navigationBuilder.Property("_contentId")
          .HasColumnName(nameof(DocumentHeader.ContentId))
          .UsePropertyAccessMode(PropertyAccessMode.Field);
      });
    });
  }
}