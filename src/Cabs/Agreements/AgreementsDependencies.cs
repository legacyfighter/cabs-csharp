using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Agreements;

public class AgreementsDependencies
{
  public static void AddTo(WebApplicationBuilder webApplicationBuilder)
  {
    webApplicationBuilder.Services.AddTransient<IContractRepository, EfCoreContractRepository>();
    webApplicationBuilder.Services
      .AddTransient<IContractAttachmentDataRepository, EfCoreContractAttachmentDataRepository>();
    webApplicationBuilder.Services.AddTransient<ContractService>();
    webApplicationBuilder.Services.AddTransient<IContractService>(ctx =>
      new TransactionalContractService(
        ctx.GetRequiredService<ContractService>(),
        ctx.GetRequiredService<ITransactions>()));
  }
}