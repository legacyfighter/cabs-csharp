using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class ContractAttachment : BaseEntity
{

  public enum Statuses
  {
    Proposed,
    AcceptedByOneSide,
    AcceptedByBothSides,
    Rejected
  }

  public ContractAttachment()
  {
  }

  public byte[] Data { get; set; }
  public Instant CreationDate { get; set; } = SystemClock.Instance.GetCurrentInstant();
  public Instant AcceptedAt { get; set; }
  public Instant RejectedAt { get; set; }
  public Instant ChangeDate { get; set; }
  public Statuses Status { get; set; } = Statuses.Proposed;
  public virtual Contract Contract { get; set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as ContractAttachment)?.Id;
  }

  public static bool operator ==(ContractAttachment left, ContractAttachment right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(ContractAttachment left, ContractAttachment right)
  {
    return !Equals(left, right);
  }
}