using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.CarFleet;

public class CarFleetDependencies
{
  public static void AddTo(WebApplicationBuilder webApplicationBuilder)
  {
    webApplicationBuilder.Services.AddTransient<ICarTypeEntityRepository, EfCoreCarTypeRepository>();
    webApplicationBuilder.Services.AddTransient<ICarTypeRepository, CarTypeRepository>();
    webApplicationBuilder.Services
      .AddTransient<ICarTypeActiveCounterRepository, EfCoreCarTypeActiveCounterRepository>();
    webApplicationBuilder.Services.AddTransient<CarTypeService>();
    webApplicationBuilder.Services.AddTransient<ICarTypeService>(
      ctx => new TransactionalCarTypeService(
        ctx.GetRequiredService<CarTypeService>(),
        ctx.GetRequiredService<ITransactions>()));
  }
}