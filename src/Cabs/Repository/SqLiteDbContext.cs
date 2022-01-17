using System.Data.Common;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Entity.Miles;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace LegacyFighter.Cabs.Repository;

public class SqLiteDbContext : DbContext
{
  private readonly DbConnection _connection;
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
  public DbSet<Driver> Drivers { get; set; }
  public DbSet<DriverAttribute> DriverAttributes { get; set; }
  public DbSet<DriverFee> DriverFees { get; set; }
  public DbSet<DriverPosition> DriverPositions { get; set; }
  public DbSet<DriverSession> DriverSessions { get; set; }
  public DbSet<Invoice> Invoices { get; set; }
  public DbSet<Transit> Transits { get; set; }
  public DbSet<ClaimsResolver> ClaimsResolvers { get; set; }

  public static DbConnection CreateInMemoryDatabase()
  {
    var connection = new SqliteConnection("Filename=:memory:");
    connection.Open();
    return connection;
  }

  public SqLiteDbContext(DbConnection connection)
  {
    _connection = connection;
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder.UseLazyLoadingProxies().UseSqlite(_connection);
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
      builder.HasOne(m => m.Transit);
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
      builder.HasOne(c => c.Transit);
      builder.Property(x => x.ChangeDate).HasConversion(instantConverter);
      builder.Property(x => x.CompletionDate).HasConversion(instantConverter);
      builder.Property(x => x.CreationDate).HasConversion(instantConverter).IsRequired();
      builder.Property(x => x.Reason).IsRequired();
      builder.Property(x => x.CompletionMode).HasConversion<string>();
      builder.Property(x => x.Status).HasConversion<string>().IsRequired();
      builder.Property(x => x.ClaimNo).IsRequired();
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
      builder.Property(x => x.Data).HasColumnType("BLOB");
      builder.Property(x => x.CreationDate).HasConversion(instantConverter).IsRequired();
      builder.Property(x => x.AcceptedAt).HasConversion(instantConverter);
      builder.Property(x => x.ChangeDate).HasConversion(instantConverter);
      builder.Property(x => x.RejectedAt).HasConversion(instantConverter);
      builder.Property(x => x.Status).HasConversion<string>();
      builder.HasOne("Contract").WithMany("Attachments");
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
      e.HasMany(d => d.Transits).WithOne(t => t.Driver);
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
      builder.Property(x => x.AcceptedAt).HasConversion(instantConverter);
      builder.Property(x => x.CompleteAt).HasConversion(instantConverter);
      builder.Property(x => x.DateTime).HasConversion(instantConverter);
      builder.Property(x => x.Date).HasConversion(instantConverter);
      builder.Property(x => x.Published).HasConversion(instantConverter);
      builder.Property(x => x.Started).HasConversion(instantConverter);
      builder.Property(x => x.CarType).HasConversion<string>();
      builder.HasOne(t => t.To);
      builder.HasOne(t => t.Client);
      builder.HasOne(t => t.Driver).WithMany(d => d.Transits);
      builder.HasMany(t => t.ProposedDrivers).WithMany(d => d.ProposingTransits);
      builder.HasMany("DriversRejections").WithMany("RejectingTransits");
      builder.OwnsOne(t => t.DriversFee, navigation =>
      {
        navigation.Property(m => m.IntValue).HasColumnName(nameof(Transit.DriversFee));
      });
      builder.OwnsOne(t => t.EstimatedPrice, navigation =>
      {
        navigation.Property(m => m.IntValue).HasColumnName(nameof(Transit.EstimatedPrice));
      });
      builder.OwnsOne(t => t.Price, navigation =>
      {
        navigation.Property(m => m.IntValue).HasColumnName(nameof(Transit.Price));
      });
      builder.OwnsOne(t => t.Tariff, navigation =>
      {
        navigation.Property(m => m.BaseFee).HasColumnName(nameof(Tariff.BaseFee));
        navigation.Property(m => m.KmRate).HasColumnName(nameof(Tariff.KmRate));
        navigation.Property(m => m.Name).HasColumnName(nameof(Tariff.Name));
      });
    });
    modelBuilder.Entity<ClaimsResolver>(builder =>
    {
      builder.MapBaseEntityProperties();
      builder.Property("ClientId");
      builder.Property("ClaimedTransitsIds");
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