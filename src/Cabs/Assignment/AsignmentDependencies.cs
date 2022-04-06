using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Assignment;

public class AsignmentDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<IDriverAssignmentRepository, EfCoreDriverAssignmentRepository>();
    builder.Services.AddTransient<DriverAssignmentFacade>();
    builder.Services.AddTransient<IDriverAssignmentFacade>(ctx =>
      new TransactionalDriverAssignmentFacade(
        ctx.GetRequiredService<DriverAssignmentFacade>(),
        ctx.GetRequiredService<ITransactions>()));

  }
}