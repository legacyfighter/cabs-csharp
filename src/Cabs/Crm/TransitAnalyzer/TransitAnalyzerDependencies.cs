using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Crm.TransitAnalyzer;

public static class TransitAnalyzerDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<GraphTransitAnalyzer>();
    builder.Services.AddTransient<PopulateGraphService>();
    builder.Services.AddTransient<IPopulateGraphService>(ctx => 
      new TransactionalPopulateGraphService(
        ctx.GetRequiredService<PopulateGraphService>(),
        ctx.GetRequiredService<ITransactions>()));
  }
}