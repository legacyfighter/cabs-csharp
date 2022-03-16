namespace LegacyFighter.Cabs.Contracts.Model.State.Straightforward.Acme;

public class DraftState : BaseState
{
  //BAD IDEA!
  //public BaseState Publish()
  //{
  //  if some validation
  //    return new PublishedState();
  //}

  protected override bool CanChangeContent()
  {
    return true;
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