using System.Data.Common;
using LegacyFighter.Cabs.Agreements;
using LegacyFighter.Cabs.Assignment;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Contracts;
using LegacyFighter.Cabs.Contracts.Legacy;
using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Contracts.Model.Content;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Crm.Claims;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.DriverFleet.DriverReports.TravelledDistances;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.Invoicing;
using LegacyFighter.Cabs.Loyalty;
using LegacyFighter.Cabs.Parties;
using LegacyFighter.Cabs.Parties.Model.Parties;
using LegacyFighter.Cabs.Ride;
using LegacyFighter.Cabs.Ride.Details;
using LegacyFighter.Cabs.Tracking;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
  public DbSet<RequestForTransit> RequestsForTransit { get; set; }
  public DbSet<DriverAssignment> DriverAssignments { get; set; }
  public DbSet<TravelledDistance> TravelledDistances { get; set; }
  public DbSet<Party> Parties { get; set; }
  public DbSet<PartyRelationship> PartyRelationships { get; set; }
  public DbSet<PartyRole> PartyRoles { get; set; }
  public DbSet<TransitDemand> TransitDemands { get; set; }
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

    RideSchema.MapUsing(modelBuilder, instantConverter);
    AgreementsSchema.MapUsing(modelBuilder, instantConverter);
    ClaimSchema.MapUsing(modelBuilder, instantConverter);
    CarFleetSchema.MapUsing(modelBuilder);
    InvoicingSchema.MapUsing(modelBuilder);
    DriverFleetSchema.MapUsing(modelBuilder, instantConverter);
    LoyaltySchema.MapUsing(modelBuilder, instantConverter);
    GeolocationSchema.MapUsing(modelBuilder);
    CrmSchema.MapUsing(modelBuilder);
    TrackingSchema.MapUsing(modelBuilder, instantConverter);
    AssignmentSchema.MapUsing(modelBuilder, instantConverter);
    PartiesSchema.MapUsing(modelBuilder);
    RepairSchema.MapUsing(modelBuilder);
    ContractsSchema.MapUsing(modelBuilder);
  }
}