using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class Contract : BaseEntity
{

  public enum Statuses
  {
    NegotiationsInProgress,
    Rejected,
    Accepted
  }

  public Contract()
  {
  }

  public Instant CreationDate { get; set; } = SystemClock.Instance.GetCurrentInstant();
  public Instant? AcceptedAt { get; set; }
  public Instant? RejectedAt { get; set; }
  public Instant? ChangeDate { get; set; }
  public Statuses Status { get; set; } = Statuses.NegotiationsInProgress;
  public string ContractNo { get; set; }
  public virtual ISet<ContractAttachment> Attachments { get; set; } = new HashSet<ContractAttachment>();
  public string PartnerName { get; set; }
  public string Subject { get; set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as Contract)?.Id;
  }

  public static bool operator ==(Contract left, Contract right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(Contract left, Contract right)
  {
    return !Equals(left, right);
  }
}