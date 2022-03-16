namespace LegacyFighter.Cabs.Contracts.Model.State.Straightforward.Acme;

public class PublishedState : BaseState
{
  protected override bool CanChangeContent()
  {
    return false;
  }

  protected override BaseState StateAfterContentChange()
  {
    return this;
  }

  protected override bool CanChangeFrom(BaseState previousState)
  {
    return previousState is VerifiedState
           && previousState.GetDocumentHeader().NotEmpty();
  }

  protected override void Acquire(DocumentHeader documentHeader)
  {
  }
}