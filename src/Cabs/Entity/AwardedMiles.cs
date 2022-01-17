using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class AwardedMiles : BaseEntity
{
  public AwardedMiles()
  {
  }

  public virtual Client Client { get; set; }
  public int Miles { get; set; }
  public Instant Date { get; set; } = SystemClock.Instance.GetCurrentInstant();
  public Instant? ExpirationDate { get; set; }
  public bool CantExpire { get; set; }
  public virtual Transit Transit { get; set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as AwardedMiles)?.Id;
  }

  public static bool operator ==(AwardedMiles left, AwardedMiles right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(AwardedMiles left, AwardedMiles right)
  {
    return !Equals(left, right);
  }
}