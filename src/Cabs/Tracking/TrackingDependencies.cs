using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Tracking;

public static class TrackingDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<IDriverSessionRepository, EfCoreDriverSessionRepository>();
    builder.Services.AddTransient<IDriverPositionRepository, EfCoreDriverPositionRepository>();
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
  }
}