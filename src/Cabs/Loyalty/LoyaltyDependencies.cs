using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Loyalty;

public static class LoyaltyDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<IAwardsAccountRepository, EfCoreAwardsAccountRepository>();
    builder.Services.AddTransient<AwardsServiceImpl>();
    builder.Services.AddTransient<IAwardsService>(ctx =>
      new TransactionalAwardsService(
        ctx.GetRequiredService<AwardsServiceImpl>(),
        ctx.GetRequiredService<ITransactions>()));
  }
}