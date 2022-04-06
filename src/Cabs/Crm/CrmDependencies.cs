using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Crm;

public static class CrmDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<IClientRepository, EfCoreClientRepository>();
    builder.Services.AddTransient<ClientService>();
    builder.Services.AddTransient<IClientService>(
      ctx => new TransactionalClientService(
        ctx.GetRequiredService<ClientService>(), 
        ctx.GetRequiredService<ITransactions>()));
  }
}