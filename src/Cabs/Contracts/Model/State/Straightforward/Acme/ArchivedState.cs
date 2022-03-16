namespace LegacyFighter.Cabs.Contracts.Model.State.Straightforward.Acme;

public class ArchivedState : BaseState
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
    return true;
  }

  protected override void Acquire(DocumentHeader documentHeader)
  {
  }
}