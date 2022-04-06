using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Crm.Claims;

public static class ClaimDependencies
{
  public static void AddTo(WebApplicationBuilder webApplicationBuilder)
  {
    webApplicationBuilder.Services.AddTransient<ClaimNumberGenerator>();
    webApplicationBuilder.Services.AddTransient<IClaimRepository, EfCoreClaimRepository>();
    webApplicationBuilder.Services.AddTransient<IClaimsResolverRepository, EfCoreClaimsResolverRepository>();
    webApplicationBuilder.Services.AddTransient<ClaimService>();
    webApplicationBuilder.Services.AddTransient<IClaimService>(ctx =>
      new CircularDependencyClaimServiceProxy(() =>
        new TransactionalClaimService(
          ctx.GetRequiredService<ClaimService>(),
          ctx.GetRequiredService<ITransactions>())));
  }
}