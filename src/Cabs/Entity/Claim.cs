using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class Claim : BaseEntity
{

  public enum Statuses
  {
    Draft,
    New,
    InProcess,
    Refunded,
    Escalated,
    Rejected
  }

  public enum CompletionModes
  {
    Manual,
    Automatic
  }

  public Claim()
  {

  }

  public string ClaimNo { get; set; }
  public virtual Client Owner { get; set; }
  public virtual Transit Transit { get; set; }
  public Instant CreationDate { get; set; }
  public Instant? CompletionDate { get; set; }
  public string IncidentDescription { get; set; }
  public CompletionModes? CompletionMode { get; set; }
  public Statuses? Status { get; set; }
  public Instant? ChangeDate { get; set; }
  public string Reason { get; set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as Claim)?.Id;
  }

  public static bool operator ==(Claim left, Claim right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(Claim left, Claim right)
  {
    return !Equals(left, right);
  }
}