using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.DriverFleet.DriverReports;
using LegacyFighter.Cabs.DriverFleet.DriverReports.TravelledDistances;

namespace LegacyFighter.Cabs.DriverFleet;

public static class DriverFleetDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<IDriverRepository, EfCoreDriverRepository>();
    builder.Services.AddTransient<IDriverFeeRepository, EfCoreDriverFeeRepository>();
    builder.Services.AddTransient<IDriverAttributeRepository, EfCoreDriverAttributeRepository>();
    builder.Services.AddTransient<ITravelledDistanceRepository, EfCoreTravelledDistanceRepository>();
    builder.Services.AddTransient<SqlBasedDriverReportCreator>();
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
    builder.Services.AddTransient<TravelledDistanceService>();
    builder.Services.AddTransient<ITravelledDistanceService>(ctx =>
      new TransactionalTravelledDistanceService(
        ctx.GetRequiredService<TravelledDistanceService>(),
        ctx.GetRequiredService<ITransactions>()));
  }
}