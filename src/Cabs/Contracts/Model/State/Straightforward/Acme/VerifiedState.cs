namespace LegacyFighter.Cabs.Contracts.Model.State.Straightforward.Acme;

public class VerifiedState : BaseState
{
  private readonly long? _verifierId;

  public VerifiedState(long? verifierId)
  {
    _verifierId = verifierId;
  }

  public VerifiedState()
  {
  }

  protected override bool CanChangeContent()
  {
    return true;
  }

  protected override BaseState StateAfterContentChange()
  {
    return new DraftState();
  }

  protected override bool CanChangeFrom(BaseState previousState)
  {
    return previousState is DraftState
           && !previousState.GetDocumentHeader().AuthorId.Equals(_verifierId)
           && previousState.GetDocumentHeader().NotEmpty();
  }

  protected override void Acquire(DocumentHeader documentHeader)
  {
    documentHeader.VerifierId = _verifierId;
  }
}