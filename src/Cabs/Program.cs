using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using NodaTime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton(_ => SqLiteDbContext.CreateInMemoryDatabase());
builder.Services.AddDbContext<SqLiteDbContext>();
builder.Services.AddTransient<ITransactions, Transactions>();
builder.Services.AddTransient<IAddressRepositoryInterface, EfCoreAddressRepository>();
builder.Services.AddTransient<IDriverRepository, EfCoreDriverRepository>();
builder.Services.AddTransient<IDriverFeeRepository, EfCoreDriverFeeRepository>();
builder.Services.AddTransient<IDriverAttributeRepository, EfCoreDriverAttributeRepository>();
builder.Services.AddTransient<IDriverSessionRepository, EfCoreDriverSessionRepository>();
builder.Services.AddTransient<IDriverPositionRepository, EfCoreDriverPositionRepository>();
builder.Services.AddTransient<IClientRepository, EfCoreClientRepository>();
builder.Services.AddTransient<ITransitRepository, EfCoreTransitRepository>();
builder.Services.AddTransient<IClaimRepository, EfCoreClaimRepository>();
builder.Services.AddTransient<IAwardsAccountRepository, EfCoreAwardsAccountRepository>();
builder.Services.AddTransient<IAwardedMilesRepository, EfCoreAwardedMilesRepository>();
builder.Services.AddTransient<IInvoiceRepository, EfCoreInvoiceRepository>();
builder.Services.AddTransient<IContractRepository, EfCoreContractRepository>();
builder.Services.AddTransient<IContractAttachmentRepository, EfCoreContractAttachmentRepository>();
builder.Services.AddTransient<ICarTypeRepository, EfCoreCarTypeRepository>();
builder.Services.AddTransient<ClaimService>();
builder.Services.AddTransient<IClaimService>(ctx => 
  new TransactionalClaimService(
    ctx.GetRequiredService<ClaimService>(), 
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<AwardsServiceImpl>();
builder.Services.AddTransient<IAwardsService>(ctx =>
  new TransactionalAwardsService(
    ctx.GetRequiredService<AwardsServiceImpl>(),
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<IClientNotificationService, ClientNotificationService>();
builder.Services.AddTransient<IDriverNotificationService, DriverNotificationService>();
builder.Services.AddTransient<CarTypeService>();
builder.Services.AddTransient<ICarTypeService>(
  ctx => new TransactionalCarTypeService(
    ctx.GetRequiredService<CarTypeService>(), 
    ctx.GetRequiredService<ITransactions>()));
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
builder.Services.AddTransient<ContractService>();
builder.Services.AddTransient<IContractService>(ctx => 
  new TransactionalContractService(
    ctx.GetRequiredService<ContractService>(), 
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<TransitService>();
builder.Services.AddTransient<ITransitService>(ctx =>
  new TransactionalTransitService(
    ctx.GetRequiredService<TransitService>(),
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<TransitAnalyzer>();
builder.Services.AddTransient<ITransitAnalyzer>(ctx =>
  new TransactionalTransitAnalyzer(
    ctx.GetRequiredService<TransitAnalyzer>(),
    ctx.GetRequiredService<ITransactions>()));
builder.Services.AddTransient<InvoiceGenerator>();
builder.Services.AddTransient<DistanceCalculator>();
builder.Services.AddTransient<ClaimNumberGenerator>();
builder.Services.AddSingleton<IAppProperties, AppProperties>();
builder.Services.AddSingleton<IClock>(_ => SystemClock.Instance);
builder.Services.AddTransient<AddressRepository>();
builder.Services.AddControllers().AddControllersAsServices();

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