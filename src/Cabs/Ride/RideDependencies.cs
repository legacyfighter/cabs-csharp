using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Ride.Details;

namespace LegacyFighter.Cabs.Ride;

public static class RideDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<TransitDetailsFacade>();
    builder.Services.AddTransient<ITransitDetailsFacade>(ctx =>
      new TransactionalTransitDetailsFacade(
        ctx.GetRequiredService<TransitDetailsFacade>(),
        ctx.GetRequiredService<ITransactions>()));
    builder.Services.AddTransient<ITransitRepository, EfCoreTransitRepository>();
    builder.Services.AddTransient<IRequestForTransitRepository, EfCoreRequestForTransitRepository>();
    builder.Services.AddTransient<ITransitDetailsRepository, EfCoreTransitDetailsRepository>();
    builder.Services.AddTransient<ITransitDemandRepository, EfCoreTransitDemandRepository>();
    builder.Services.AddTransient<RideService>();
    builder.Services.AddTransient<IRideService>(ctx =>
      new TransactionalRideService(
        ctx.GetRequiredService<RideService>(),
        ctx.GetRequiredService<ITransactions>()));
    builder.Services.AddTransient<RequestTransitService>();
    builder.Services.AddTransient<IRequestTransitService>(ctx =>
      new TransactionalRequestTransitService(
        ctx.GetRequiredService<RequestTransitService>(),
        ctx.GetRequiredService<ITransactions>()));
    builder.Services.AddTransient<ChangeDestinationService>();
    builder.Services.AddTransient<IChangeDestinationService>(ctx =>
      new TransactionalChangeDestinationService(
        ctx.GetRequiredService<ChangeDestinationService>(),
        ctx.GetRequiredService<ITransactions>()));
    builder.Services.AddTransient<ChangePickupService>();
    builder.Services.AddTransient<IChangePickupService>(ctx =>
      new TransactionalChangePickupService(
        ctx.GetRequiredService<ChangePickupService>(),
        ctx.GetRequiredService<ITransactions>()));
  }
}