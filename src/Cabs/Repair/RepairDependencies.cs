using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Repair.Api;
using LegacyFighter.Cabs.Repair.Legacy.Dao;
using LegacyFighter.Cabs.Repair.Legacy.Service;

namespace LegacyFighter.Cabs.Repair;

public class RepairDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
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
  }
}