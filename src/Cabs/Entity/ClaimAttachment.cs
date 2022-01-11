using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class ClaimAttachment : BaseEntity
{
  protected ClaimAttachment()
  {

  }

  public Client Client => Claim.Owner;
  public virtual Claim Claim { get; set; }
  public Instant CreationDate { get; set; }
  public string Description { get; set; }
  public byte[] Data { get; set; }

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