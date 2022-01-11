using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class AwardsAccount : BaseEntity
{
  public AwardsAccount()
  {
  }

  public virtual Client Client { set; get; }
  public Instant Date { set; get; } = SystemClock.Instance.GetCurrentInstant();
  public bool Active { set; get; } = false;
  public int Transactions { get; private set; } = 0;

  public void IncreaseTransactions()
  {
    Transactions++;
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as AwardsAccount)?.Id;
  }

  public static bool operator ==(AwardsAccount left, AwardsAccount right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(AwardsAccount left, AwardsAccount right)
  {
    return !Equals(left, right);
  }
}