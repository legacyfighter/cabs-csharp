using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Contracts.Application;
using LegacyFighter.Cabs.Contracts.Application.Acme.Dynamic;
using LegacyFighter.Cabs.Contracts.Application.Acme.Straightforward;
using LegacyFighter.Cabs.Contracts.Application.Editor;
using LegacyFighter.Cabs.Contracts.Infra;
using LegacyFighter.Cabs.Contracts.Legacy;
using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Contracts.Model.Content;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Acme;

namespace LegacyFighter.Cabs.Contracts;

public static class ContractsDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<IApplicationEventPublisher, MediatRApplicationEventPublisher>();
    builder.Services.AddTransient<IUserRepository, EfCoreUserRepository>();
    builder.Services.AddTransient<IDocumentContentRepository, EfCoreDocumentContentRepository>();
    builder.Services.AddTransient<IDocumentHeaderRepository, EfCoreDocumentHeaderRepository>();
    builder.Services.AddTransient<DocumentEditor>();
    builder.Services.AddTransient<IDocumentEditor>(ctx => new TransactionalDocumentEditor(
      ctx.GetRequiredService<DocumentEditor>(),
      ctx.GetRequiredService<ITransactions>()));
    builder.Services.AddTransient<DocumentResourceManager>();
    builder.Services.AddTransient<IDocumentResourceManager>(ctx => 
      new TransactionalDocumentResourceManager(
        ctx.GetRequiredService<DocumentResourceManager>(),
        ctx.GetRequiredService<ITransactions>()));
    builder.Services.AddTransient<AcmeContractProcessBasedOnStraightforwardDocumentModel>();
    builder.Services.AddTransient<IAcmeContractProcessBasedOnStraightforwardDocumentModel>(ctx =>
      new TransactionalAcmeContractProcessBasedOnStraightforwardDocumentModel(
        ctx.GetRequiredService<AcmeContractProcessBasedOnStraightforwardDocumentModel>(),
        ctx.GetRequiredService<ITransactions>()));
    builder.Services.AddTransient<AcmeContractStateAssembler>();
    builder.Services.AddTransient<AcmeStateFactory>();
  }
}