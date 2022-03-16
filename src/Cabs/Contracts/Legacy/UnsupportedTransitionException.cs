namespace LegacyFighter.Cabs.Contracts.Legacy;

public class UnsupportedTransitionException : Exception
{
  public UnsupportedTransitionException(DocumentStatus current, DocumentStatus desired)
    : base("can not transit form " + current + " to " + desired)
  {
    Current = current;
    Desired = desired;
  }

  public DocumentStatus Current { get; }
  public DocumentStatus Desired { get; }
}