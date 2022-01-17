 namespace LegacyFighter.Cabs.Common;

public class BaseEntity
{
  private int? Version { get; set; }

  public override int GetHashCode()
  {
    return GetType().GetHashCode();
  }

  public long? Id { get; protected set; }
}