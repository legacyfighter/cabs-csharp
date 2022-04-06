using LegacyFighter.Cabs.Agreements;
using LegacyFighter.Cabs.Assignment;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Contracts;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Crm.Claims;
using LegacyFighter.Cabs.Crm.TransitAnalyzer;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Invoicing;
using LegacyFighter.Cabs.Loyalty;
using LegacyFighter.Cabs.Notification;
using LegacyFighter.Cabs.Parties;
using LegacyFighter.Cabs.Pricing;
using LegacyFighter.Cabs.Repair;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Ride;
using LegacyFighter.Cabs.Tracking;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Neo4j.Driver;
using NodaTime;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddDbContext<SqLiteDbContext>();
builder.Services.AddTransient<DbContext>(ctx => ctx.GetRequiredService<SqLiteDbContext>());
builder.Services.AddScoped<EventsPublisher>();
builder.Services.AddTransient<ITransactions, Transactions>();
builder.Services.AddSingleton<IAppProperties, AppProperties>();
builder.Services.AddSingleton<IClock>(_ => SystemClock.Instance);
builder.Services.AddFeatureManagement();
builder.Services.AddControllers().AddControllersAsServices();
ClaimDependencies.AddTo(builder);
AgreementsDependencies.AddTo(builder);
CarFleetDependencies.AddTo(builder);
InvoicingDependencies.AddTo(builder);
TransitAnalyzerDependencies.AddTo(builder);
NotificationDependencies.AddTo(builder);
DriverFleetDependencies.AddTo(builder);
LoyaltyDependencies.AddTo(builder);
GeolocationDependencies.AddTo(builder);
CrmDependencies.AddTo(builder);
TrackingDependencies.AddTo(builder);
PricingDependencies.AddTo(builder);
AsignmentDependencies.AddTo(builder);
RideDependencies.AddTo(builder);
ContractsDependencies.AddTo(builder);
PartiesDependencies.AddTo(builder);
RepairDependencies.AddTo(builder);

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