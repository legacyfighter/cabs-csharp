using LegacyFighter.Cabs.Contracts.Application;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Actions;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Events;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Predicates.StateChange;

namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Acme;

/// <summary>
/// Sample static config.
/// </summary>
public class AcmeContractStateAssembler
{
  public const string Verified = "verified";
  public const string Draft = "draft";
  public const string Published = "published";
  public const string Archived = "archived";

  public const string ParamVerifier = ChangeVerifier.ParamVerifier;

  private readonly IApplicationEventPublisher _publisher;

  public AcmeContractStateAssembler(IApplicationEventPublisher publisher)
  {
    _publisher = publisher;
  }

  public IStateConfig Assemble()
  {
    var builder = new StateBuilder();
    builder.BeginWith(Draft).Check(new ContentNotEmptyVerifier()).Check(new AuthorIsNotAVerifier()).To(Verified)
      .Action(new ChangeVerifier());
    builder.From(Draft).WhenContentChanged().To(Draft);
    //name of the "published" state and name of the DocumentPublished event are NOT correlated. These are two different domains, name similarity is just a coincidence
    builder.From(Verified).Check(new ContentNotEmptyVerifier()).To(Published)
      .Action(new PublishEvent<DocumentPublished>(_publisher));
    builder.From(Verified).WhenContentChanged().To(Draft);
    builder.From(Draft).To(Archived);
    builder.From(Verified).To(Archived);
    builder.From(Published).To(Archived).Action(new PublishEvent<DocumentUnpublished>(_publisher));
    return builder;
  }
}