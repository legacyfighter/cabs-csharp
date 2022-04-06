using System.Data.Common;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Contracts.Legacy;
using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Contracts.Model.Content;
using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.DriverReports.TravelledDistances;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Entity.Miles;
using LegacyFighter.Cabs.Parties.Model.Parties;
using LegacyFighter.Cabs.Repair.Legacy.Parts;
using LegacyFighter.Cabs.Repair.Legacy.User;
using LegacyFighter.Cabs.TransitDetail;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace LegacyFighter.Cabs.Repository;

public class SqLiteDbContext : DbContext
{
  private readonly DbConnection _connection;
  private readonly EventsPublisher _eventsPublisher;
  public DbSet<Address> Addresses { get; set; }
  public DbSet<AwardedMiles> AwardedMiles { get; set; }
  public DbSet<AwardsAccount> AwardsAccounts { get; set; }
  public DbSet<CarType> CarTypes { get; set; }
  public DbSet<CarTypeActiveCounter> CarTypeActiveCounters { get; set; }
  public DbSet<Claim> Claims { get; set; }
  public DbSet<ClaimAttachment> ClaimAttachments { get; set; }
  public DbSet<Client> Clients { get; set; }
  public DbSet<Contract> Contracts { get; set; }
  public DbSet<ContractAttachment> ContractAttachments { get; set; }
  public DbSet<ContractAttachmentData> ContractAttachmentsData { get; set; }
  public DbSet<Driver> Drivers { get; set; }
  public DbSet<DriverAttribute> DriverAttributes { get; set; }
  public DbSet<DriverFee> DriverFees { get; set; }
  public DbSet<DriverPosition> DriverPositions { get; set; }
  public DbSet<DriverSession> DriverSessions { get; set; }
  public DbSet<Invoice> Invoices { get; set; }
  public DbSet<Transit> Transits { get; set; }
  public DbSet<TransitDetails> TransitsDetails { get; set; }
  public DbSet<ClaimsResolver> ClaimsResolvers { get; set; }
  public DbSet<TravelledDistance> TravelledDistances { get; set; }
  public DbSet<Party> Parties { get; set; }
  public DbSet<PartyRelationship> PartyRelationships { get; set; }
  public DbSet<PartyRole> PartyRoles { get; set; }
  public DbSet<User> Users { get; set; }
  public DbSet<DocumentContent> DocumentContents { get; set; }
  public DbSet<DocumentHeader> DocumentHeaders { get; set; }

  public static DbConnection CreateInMemoryDatabase()
  {
    var connection = new SqliteConnection("Filename=:memory:");
    connection.Open();
    return connection;
  }

  public SqLiteDbContext(DbConnection connection, EventsPublisher eventsPublisher)
  {
    _connection = connection;
    _eventsPublisher = eventsPublisher;
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder.UseLazyLoadingProxies().UseSqlite(_connection);
    optionsBuilder.AddInterceptors(new List<IInterceptor>
    {
      new EventFlushInterceptor(_eventsPublisher)
    });
    optionsBuilder
      .LogTo(Console.WriteLine)
      .EnableSensitiveDataLogging()
      .EnableDetailedErrors();
    base.OnConfiguring(optionsBuilder);
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    var instantConverter = new ValueConverter<Instant,long>(
        instant => instant.ToUnixTimeTicks(),
        ticks => Instant.FromUnixTimeTicks(ticks));

    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Address>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.HasIndex(u => u.Hash).IsUnique();
    });
    modelBuilder.Entity<AwardedMiles>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.HasOne(m => m.Client);
      builder.Property("MilesJson").IsRequired();
      builder.Ignore(m => m.Miles);
      builder.Property(m => m.TransitId);
      builder.HasOne<AwardsAccount>("Account").WithMany("Miles");
      builder.Property(x => x.Date).HasConversion(instantConverter).IsRequired();
      builder.Ignore(x => x.ExpirationDate);
    });
    modelBuilder.Entity<AwardsAccount>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.HasOne(a => a.Client);
      builder.HasMany<AwardedMiles>("Miles").WithOne("Account");
      builder.Property(x => x.Date).HasConversion(instantConverter).IsRequired();
      builder.Property(x => x.Transactions).IsRequired();
      builder.Property(x => x.Active).IsRequired();
    });
    modelBuilder.Entity<CarType>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(t => t.CarClass).HasConversion<string>().IsRequired();
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
    modelBuilder.Entity<Claim>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.HasOne(c => c.Owner).WithMany(c => c.Claims);
      builder.Property(c => c.TransitId);
      builder.Property(x => x.ChangeDate).HasConversion(instantConverter);
      builder.Property(x => x.CompletionDate).HasConversion(instantConverter);
      builder.Property(x => x.CreationDate).HasConversion(instantConverter).IsRequired();
      builder.Property(x => x.Reason).IsRequired();
      builder.Property(x => x.CompletionMode).HasConversion<string>();
      builder.Property(x => x.Status).HasConversion<string>().IsRequired();
      builder.Property(x => x.ClaimNo).IsRequired();
      builder.OwnsOne(x => x.TransitPrice, navigation =>
      {
        navigation.Property(m => m.IntValue).HasColumnName(nameof(Claim.TransitPrice)).IsRequired();
      });
    });
    modelBuilder.Entity<ClaimAttachment>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.HasOne(a => a.Claim);
      builder.Property(x => x.CreationDate).HasConversion(instantConverter).IsRequired();
      builder.Property(x => x.Data).HasColumnType("BLOB");
    });
    modelBuilder.Entity<Client>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(c => c.ClientType).HasConversion<string>();
      builder.Property(c => c.DefaultPaymentType).HasConversion<string>();
      builder.HasMany(c => c.Claims).WithOne(c => c.Owner);
    });
    modelBuilder.Entity<Contract>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.HasMany("Attachments").WithOne("Contract");
      builder.Property(x => x.CreationDate).HasConversion(instantConverter).IsRequired();
      builder.Property(x => x.AcceptedAt).HasConversion(instantConverter);
      builder.Property(x => x.ChangeDate).HasConversion(instantConverter);
      builder.Property(x => x.RejectedAt).HasConversion(instantConverter);
      builder.Property(x => x.Status).HasConversion<string>().IsRequired();
      builder.Property(x => x.ContractNo).IsRequired();
    });
    modelBuilder.Entity<ContractAttachment>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(x => x.ContractAttachmentNo).IsRequired();
      builder.Property(x => x.AcceptedAt).HasConversion(instantConverter);
      builder.Property(x => x.ChangeDate).HasConversion(instantConverter);
      builder.Property(x => x.RejectedAt).HasConversion(instantConverter);
      builder.Property(x => x.Status).HasConversion<string>();
      builder.HasOne("Contract").WithMany("Attachments");
    });
    modelBuilder.Entity<ContractAttachmentData>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(x => x.Data).HasColumnType("BLOB");
      builder.Property(x => x.ContractAttachmentNo).IsRequired();
      builder.Property(x => x.CreationDate).HasConversion(instantConverter).IsRequired();
    });
    modelBuilder.Entity<Driver>(e =>
    {
      e.MapBaseEntityProperties();
      e.Property(d => d.Status).HasConversion<string>().IsRequired();
      e.Property(d => d.Type).HasConversion<string>();
      e.OwnsOne(driver => driver.DriverLicense, builder =>
      {
        builder.Property(dl => dl.ValueAsString).HasColumnName(nameof(Driver.DriverLicense)).IsRequired();
      });
      e.HasMany(d => d.Attributes).WithOne(d => d.Driver);
      e.HasOne(d => d.Fee).WithOne(f => f.Driver).HasForeignKey<DriverFee>(x => x.Id);
    });
    modelBuilder.Entity<DriverAttribute>(builder =>
    {
      builder.HasKey(x => x.Id);
      builder.Property(a => a.Name).HasConversion<string>().IsRequired();
      builder.Property(a => a.Value).IsRequired();
      builder.HasOne(a => a.Driver).WithMany(d => d.Attributes);
    });
    modelBuilder.Entity<DriverFee>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(f => f.FeeType).IsRequired();
      builder.Property(f => f.Amount).IsRequired();
      builder.OwnsOne(f => f.Min, navigation =>
      {
        navigation.Property(m => m.IntValue).HasColumnName(nameof(DriverFee.Min));
      });
    });
    modelBuilder.Entity<DriverPosition>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.HasOne(p => p.Driver);
      builder.Property(p => p.Latitude).IsRequired();
      builder.Property(p => p.Longitude).IsRequired();
      builder.Property(p => p.SeenAt).HasConversion(instantConverter).IsRequired();
    });
    modelBuilder.Entity<DriverSession>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property(x => x.LoggedAt).HasConversion(instantConverter).IsRequired();
      builder.Property(x => x.LoggedOutAt).HasConversion(instantConverter);
      builder.Property(x => x.PlatesNumber).IsRequired();
      builder.Property(x => x.CarClass).HasConversion<string>();
      builder.HasOne(s => s.Driver);
    });
    modelBuilder.Entity<Invoice>(builder =>
    {
      builder.MapBaseEntityProperties();
    });
    modelBuilder.Entity<Transit>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Ignore(x => x.KmDistance);
      builder.Property("Km");
      builder.Property("PickupAddressChangeCounter");
      builder.Property(x => x.Published).HasConversion(instantConverter);
      builder.HasOne(t => t.Driver);
      builder.HasMany(t => t.ProposedDrivers).WithMany(d => d.ProposingTransits);
      builder.HasMany("DriversRejections").WithMany("RejectingTransits");
      builder.OwnsOne(t => t.EstimatedPrice, navigation =>
      {
        navigation.Property(m => m.IntValue).HasColumnName(nameof(Transit.EstimatedPrice));
      });
      builder.OwnsOne(t => t.Price, navigation =>
      {
        navigation.Property(m => m.IntValue).HasColumnName(nameof(Transit.Price));
      });
      builder.OwnsOne(t => t.Tariff, MapTariffProperties);
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
      builder.OwnsOne(d => d.DriversFee, navigationBuilder =>
      {
        navigationBuilder.Property(m => m.IntValue).HasColumnName(nameof(TransitDetails.DriversFee));
      });
      builder.OwnsOne(d => d.Price, navigationBuilder =>
      {
        navigationBuilder.Property(m => m.IntValue).HasColumnName(nameof(TransitDetails.Price));
      });
      builder.OwnsOne(d => d.EstimatedPrice, navigationBuilder =>
      {
        navigationBuilder.Property(m => m.IntValue).HasColumnName(nameof(TransitDetails.EstimatedPrice));
      });
      builder.Property(d => d.Status);
      builder.Property(d => d.PublishedAt).HasConversion(instantConverter);
      builder.Property(d => d.Distance).HasColumnName("Km")
        .HasConversion(
        distance => distance.ToKmInDouble(),
        value => Distance.OfKm(value));
      builder.Property(d => d.TransitId);
      builder.OwnsOne<Tariff>("Tariff", MapTariffProperties);
    });
    modelBuilder.Entity<ClaimsResolver>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property("ClientId");
      builder.Property("ClaimedTransitsIds");
    });
    modelBuilder.Entity<TravelledDistance>(builder =>
    {
      builder.HasKey("IntervalId");
      builder.Property<long>("_driverId").HasColumnName("DriverId").IsRequired();
      builder.Property(e => e.LastLatitude).IsRequired();
      builder.Property(e => e.LastLongitude).IsRequired();
      builder.OwnsOne<TimeSlot>("TimeSlot", navigation =>
      {
        navigation.Property(s => s.Beginning).HasColumnName("Beginning").HasConversion(instantConverter);
        navigation.Property(s => s.End).HasColumnName("End").HasConversion(instantConverter);
      });
      builder.Property<Distance>("Distance").HasColumnName("Km")
        .HasConversion(
          distance => distance.ToKmInDouble(),
          value => Distance.OfKm(value)).IsRequired();

    });

    MapRepairEntities(modelBuilder);
    MapContractEntities(modelBuilder);
  }

  private static void MapTariffProperties<T>(OwnedNavigationBuilder<T, Tariff> navigation) where T : class
  {
    navigation.Property(m => m.BaseFee).HasColumnName(nameof(Tariff.BaseFee));
    navigation.Property(m => m.KmRate).HasColumnName(nameof(Tariff.KmRate));
    navigation.Property(m => m.Name).HasColumnName(nameof(Tariff.Name));
  }

  private void MapRepairEntities(ModelBuilder modelBuilder)
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

  private void MapContractEntities(ModelBuilder modelBuilder)
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

public static class EfCoreExtensions
{
  public static void MapBaseEntityProperties<T>(this EntityTypeBuilder<T> builder) where T : BaseEntity
  {
    builder.HasKey(e => e.Id);
    builder.Property("Version").IsRowVersion();
  }
}