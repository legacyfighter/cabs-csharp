using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Agreements;

public class ContractAttachment : BaseEntity
{
  internal Guid ContractAttachmentNo { get; private set; } = Guid.NewGuid();
  internal Instant AcceptedAt { get; set; }
  internal Instant RejectedAt { get; set; }
  internal Instant ChangeDate { get; set; }
  internal ContractAttachmentStatuses Status { get; set; } = ContractAttachmentStatuses.Proposed;
  internal virtual Contract Contract { get; set; }

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