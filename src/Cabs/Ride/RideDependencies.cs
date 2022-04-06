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
    builder.Services.AddTransient<TransitService>();
    builder.Services.AddTransient<ITransitService>(ctx =>
      new TransactionalTransitService(
        ctx.GetRequiredService<TransitService>(),
        ctx.GetRequiredService<ITransactions>()));
  }
}