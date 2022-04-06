using LegacyFighter.Cabs.Agreements;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Contracts.Application;
using LegacyFighter.Cabs.Contracts.Application.Acme.Dynamic;
using LegacyFighter.Cabs.Contracts.Application.Acme.Straightforward;
using LegacyFighter.Cabs.Contracts.Application.Editor;
using LegacyFighter.Cabs.Contracts.Infra;
using LegacyFighter.Cabs.Contracts.Legacy;
using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Contracts.Model.Content;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Acme;
using LegacyFighter.Cabs.Crm.Claims;
using LegacyFighter.Cabs.DriverReports;
using LegacyFighter.Cabs.DriverReports.TravelledDistances;
using LegacyFighter.Cabs.Parties.Api;
using LegacyFighter.Cabs.Parties.Infra;
using LegacyFighter.Cabs.Parties.Model.Parties;
using LegacyFighter.Cabs.Repair.Api;
using LegacyFighter.Cabs.Repair.Legacy.Dao;
using LegacyFighter.Cabs.Repair.Legacy.Service;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using LegacyFighter.Cabs.TransitAnalyzer;
using LegacyFighter.Cabs.TransitDetail;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Neo4j.Driver;
using NodaTime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOptions();
builder.Services.AddSingleton<EventLoop>();
builder.Services.AddHostedService(ctx => ctx.GetRequiredService<EventLoop>());
builder.Services.AddMediatR(typeof(Program));
builder.Services.Configure<GraphDatabaseOptions>(
  builder.Configuration.GetSection("GraphDatabase"));
builder.Services.AddSingleton(_ => SqLiteDbContext.CreateInMemoryDatabase());
builder.Services.AddSingleton(ctx => GraphDatabase.Driver(
  ctx.GetRequiredService<IOptions<GraphDatabaseOptions>>().Value.Uri, 
  AuthTokens.None));
builder.Services.AddTransient<GraphTransitAnalyzer>();
builder.Services.AddTransient<PopulateGraphService>();
builder.Services.AddTransient<IPopulateGraphService>(ctx => 
  new TransactionalPopulateGraphService(
    ctx.GetRequiredService<PopulateGraphService>(),
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<TransitDetailsFacade>();
builder.Services.AddTransient<ITransitDetailsFacade>(ctx =>
  new TransactionalTransitDetailsFacade(
    ctx.GetRequiredService<TransitDetailsFacade>(),
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddDbContext<SqLiteDbContext>();
builder.Services.AddTransient<DbContext>(ctx => ctx.GetRequiredService<SqLiteDbContext>());
builder.Services.AddScoped<EventsPublisher>();
builder.Services.AddTransient<ITransactions, Transactions>();
builder.Services.AddTransient<IAddressRepositoryInterface, EfCoreAddressRepository>();
builder.Services.AddTransient<IDriverRepository, EfCoreDriverRepository>();
builder.Services.AddTransient<IDriverFeeRepository, EfCoreDriverFeeRepository>();
builder.Services.AddTransient<IDriverAttributeRepository, EfCoreDriverAttributeRepository>();
builder.Services.AddTransient<IDriverSessionRepository, EfCoreDriverSessionRepository>();
builder.Services.AddTransient<IDriverPositionRepository, EfCoreDriverPositionRepository>();
builder.Services.AddTransient<IClientRepository, EfCoreClientRepository>();
builder.Services.AddTransient<ITransitRepository, EfCoreTransitRepository>();
builder.Services.AddTransient<ITransitDetailsRepository, EfCoreTransitDetailsRepository>();
builder.Services.AddTransient<IAwardsAccountRepository, EfCoreAwardsAccountRepository>();
builder.Services.AddTransient<IInvoiceRepository, EfCoreInvoiceRepository>();
builder.Services.AddTransient<ITravelledDistanceRepository, EfCoreTravelledDistanceRepository>();
builder.Services.AddTransient<SqlBasedDriverReportCreator>();
builder.Services.AddTransient<AwardsServiceImpl>();
builder.Services.AddTransient<IAwardsService>(ctx =>
  new TransactionalAwardsService(
    ctx.GetRequiredService<AwardsServiceImpl>(),
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<IClientNotificationService, ClientNotificationService>();
builder.Services.AddTransient<IDriverNotificationService, DriverNotificationService>();
builder.Services.AddTransient<ClientService>();
builder.Services.AddTransient<IClientService>(
  ctx => new TransactionalClientService(
    ctx.GetRequiredService<ClientService>(), 
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<DriverService>();
builder.Services.AddTransient<IDriverService>(ctx =>
  new TransactionalDriverService(
    ctx.GetRequiredService<DriverService>(), 
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<DriverFeeService>();
builder.Services.AddTransient<IDriverFeeService>(ctx =>
  new TransactionalDriverFeeService(
    ctx.GetRequiredService<DriverFeeService>(),
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<DriverTrackingService>();
builder.Services.AddTransient<IDriverTrackingService>(ctx =>
  new TransactionalDriverTrackingService(
    ctx.GetRequiredService<DriverTrackingService>(),
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<DriverSessionService>();
builder.Services.AddTransient<IDriverSessionService>(ctx =>
  new TransactionalDriverSessionService(
    ctx.GetRequiredService<DriverSessionService>(),
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<IGeocodingService, GeocodingService>();
builder.Services.AddTransient<TransitService>();
builder.Services.AddTransient<ITransitService>(ctx =>
  new TransactionalTransitService(
    ctx.GetRequiredService<TransitService>(),
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<TravelledDistanceService>();
builder.Services.AddTransient<ITravelledDistanceService>(ctx =>
  new TransactionalTravelledDistanceService(
    ctx.GetRequiredService<TravelledDistanceService>(),
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<InvoiceGenerator>();
builder.Services.AddTransient<DistanceCalculator>();
builder.Services.AddSingleton<IAppProperties, AppProperties>();
builder.Services.AddSingleton<IClock>(_ => SystemClock.Instance);
builder.Services.AddTransient<AddressRepository>();
builder.Services.AddTransient<IAddressRepository>(ctx => 
  new TransactionalAddressRepository(
    ctx.GetRequiredService<AddressRepository>(),
    ctx.GetRequiredService<ITransactions>()));

builder.Services.AddTransient<UserDao>();
builder.Services.AddTransient<JobDoer>();
builder.Services.AddTransient<IJobDoer>(ctx => new TransactionalJobDoer(
  ctx.GetRequiredService<JobDoer>(),
  ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<ContractManager>();
builder.Services.AddTransient<IContractManager>(ctx => new TransactionalContractManager(
  ctx.GetRequiredService<ContractManager>(),
  ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<RepairProcess>();
builder.Services.AddTransient<PartyMapper>();
builder.Services.AddTransient<IPartyRepository, EfCorePartyRepository>();
builder.Services.AddTransient<IPartyRelationshipRepository, EfCorePartyRelationshipRepository>();

builder.Services.AddTransient<IApplicationEventPublisher, MediatRApplicationEventPublisher>();
builder.Services.AddTransient<IUserRepository, EfCoreUserRepository>();
builder.Services.AddTransient<IDocumentContentRepository, EfCoreDocumentContentRepository>();
builder.Services.AddTransient<IDocumentHeaderRepository, EfCoreDocumentHeaderRepository>();
builder.Services.AddTransient<DocumentEditor>();
builder.Services.AddTransient<IDocumentEditor>(ctx => new TransactionalDocumentEditor(
  ctx.GetRequiredService<DocumentEditor>(),
  ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<DocumentResourceManager>();
builder.Services.AddTransient<IDocumentResourceManager>(ctx => 
  new TransactionalDocumentResourceManager(
    ctx.GetRequiredService<DocumentResourceManager>(),
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<AcmeContractProcessBasedOnStraightforwardDocumentModel>();
builder.Services.AddTransient<IAcmeContractProcessBasedOnStraightforwardDocumentModel>(ctx =>
  new TransactionalAcmeContractProcessBasedOnStraightforwardDocumentModel(
    ctx.GetRequiredService<AcmeContractProcessBasedOnStraightforwardDocumentModel>(),
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<AcmeContractStateAssembler>();
builder.Services.AddTransient<AcmeStateFactory>();

builder.Services.AddFeatureManagement();
builder.Services.AddControllers().AddControllersAsServices();

ClaimDependencies.AddTo(builder);
AgreementsDependencies.AddTo(builder);
CarFleetDependencies.AddTo(builder);

var app = builder.Build();

using (var serviceScope = app.Services.CreateScope())
{
  var context = serviceScope.ServiceProvider.GetRequiredService<SqLiteDbContext>();
  await context.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}