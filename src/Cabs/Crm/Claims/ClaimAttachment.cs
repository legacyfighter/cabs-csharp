using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Crm.Claims;

public class ClaimAttachment : BaseEntity
{
  protected ClaimAttachment()
  {

  }

  internal long? ClientId => Claim.OwnerId;
  internal virtual Claim Claim { get; set; }
  internal Instant CreationDate { get; set; }
  internal string Description { get; set; }
  internal byte[] Data { get; set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as ClaimAttachment)?.Id;
  }

  public static bool operator ==(ClaimAttachment left, ClaimAttachment right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(ClaimAttachment left, ClaimAttachment right)
  {
    return !Equals(left, right);
  }
}