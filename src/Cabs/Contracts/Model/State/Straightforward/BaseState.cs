namespace LegacyFighter.Cabs.Contracts.Model.State.Straightforward;

//TODO introduce an interface

public abstract class BaseState
{
  protected DocumentHeader DocumentHeader;

  public void Init(DocumentHeader documentHeader)
  {
    DocumentHeader = documentHeader;
    documentHeader.StateDescriptor = StateDescriptor;
  }

  public BaseState ChangeContent(ContentId currentContent)
  {
    if (CanChangeContent())
    {
      var newState = StateAfterContentChange();
      newState.Init(DocumentHeader);
      DocumentHeader.ChangeCurrentContent(currentContent);
      newState.Acquire(DocumentHeader);
      return newState;
    }

    return this;
  }

  protected abstract bool CanChangeContent();

  protected abstract BaseState StateAfterContentChange();

  public BaseState ChangeState(BaseState newState)
  {
    if (newState.CanChangeFrom(this))
    {
      newState.Init(DocumentHeader);
      DocumentHeader.StateDescriptor = newState.StateDescriptor;
      newState.Acquire(DocumentHeader);
      return newState;
    }

    return this;
  }

  public string StateDescriptor => GetType().Name;

  /// <summary>
  /// template method that allows to perform addition actions during state change
  /// </summary>
  protected abstract void Acquire(DocumentHeader documentHeader);

  protected abstract bool CanChangeFrom(BaseState previousState);

  public DocumentHeader GetDocumentHeader()
  {
    return DocumentHeader;
  }
}