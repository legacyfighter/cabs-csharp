using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity.Miles;

public class AwardedMiles : BaseEntity
{
  public AwardedMiles()
  {
  }

  public virtual Client Client { get; set; }

  private string MilesJson { get; set; }

  public IMiles Miles
  {
    get => MilesJsonMapper.Deserialize(MilesJson);
    set => MilesJson = MilesJsonMapper.Serialize(value);
  }

  public int? GetMilesAmount(Instant when) 
  {
    return Miles.GetAmountFor(when);
  }

  public Instant Date { get; set; } = SystemClock.Instance.GetCurrentInstant();

  public Instant? ExpirationDate => Miles.ExpiresAt();

  public bool CantExpire => ExpirationDate.Value.ToUnixTimeTicks() == Instant.MaxValue.ToUnixTimeTicks();

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

  public void RemoveAll(Instant forWhen) 
  {
    Miles = Miles.Subtract(GetMilesAmount(forWhen), forWhen);
  }

  public void Subtract(int? miles, Instant forWhen) 
  {
    Miles = Miles.Subtract(miles, forWhen);
  }
}