namespace LegacyFighter.Cabs.Pricing;

public static class PricingDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<Tariffs>();
  }
}