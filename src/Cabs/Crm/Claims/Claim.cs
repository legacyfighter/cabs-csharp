using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.MoneyValue;
using NodaTime;

namespace LegacyFighter.Cabs.Crm.Claims;

public class Claim : BaseEntity
{
  public enum CompletionModes
  {
    Manual,
    Automatic
  }

  public Claim()
  {
  }

  public string ClaimNo { get; internal set; }
  public long? OwnerId { get; internal set; }
  public long? TransitId { get; internal set; }
  internal Instant CreationDate { get; set; }
  internal Instant? CompletionDate { get; set; }
  internal string IncidentDescription { get; set; }
  public CompletionModes? CompletionMode { get; internal set; }
  public Statuses? Status { get; internal set; }
  internal Instant? ChangeDate { get; set; }
  internal string Reason { get; set; }
  internal Money TransitPrice { get; set; }

  internal void Escalate() 
  {
    Status = Statuses.Escalated;
    CompletionDate = SystemClock.Instance.GetCurrentInstant();
    ChangeDate = SystemClock.Instance.GetCurrentInstant();
    CompletionMode = CompletionModes.Manual;
  }

  internal void Refund()
  {
    Status = Statuses.Refunded;
    CompletionDate = SystemClock.Instance.GetCurrentInstant();
    ChangeDate = SystemClock.Instance.GetCurrentInstant();
    CompletionMode = CompletionModes.Automatic;
  }

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