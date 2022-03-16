namespace LegacyFighter.Cabs.Contracts.Application.Editor;

public class CommitResult
{
  public enum Results
  {
    Failure,
    Success
  }

  private string _message;

  public CommitResult(Guid contentId, Results result, string message)
  {
    ContentId = contentId;
    Result = result;
    _message = message;
  }

  public CommitResult(Guid documentId, Results result) : this(documentId, result, null)
  {
  }

  public Results Result { get; }
  public Guid ContentId { get; }
}