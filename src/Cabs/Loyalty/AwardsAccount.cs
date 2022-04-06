using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Loyalty;

public class AwardsAccount : BaseEntity
{
  public static AwardsAccount NotActiveAccount(long? clientId, Instant date) 
  {
    return new AwardsAccount(clientId, false, date);
  }

  public AwardsAccount()
  {
  }

  public AwardsAccount(long? clientId, bool isActive, Instant date) 
  {
    ClientId = clientId;
    Active = isActive;
    Date = date;
  }

  public long? ClientId { get; }
  public Instant Date { get; } = SystemClock.Instance.GetCurrentInstant();
  public bool Active { private set; get; } = false;
  public int Transactions { get; private set; } = 0;

  protected virtual ISet<AwardedMiles> Miles { get; set; } = new HashSet<AwardedMiles>();

  public AwardedMiles AddExpiringMiles(int amount, Instant expireAt, long? transitId, Instant when) 
  {
    var expiringMiles = new AwardedMiles(this, transitId, ClientId, when, ConstantUntil.Value(amount, expireAt));
    Miles.Add(expiringMiles);
    Transactions++;
    return expiringMiles;
  }

  public AwardedMiles AddNonExpiringMiles(int amount, Instant when)
  {
    var nonExpiringMiles = new AwardedMiles(this, null, ClientId, when, ConstantUntil.Forever(amount));
    Miles.Add(nonExpiringMiles);
    Transactions++;
    return nonExpiringMiles;
  }

  public int? CalculateBalance(Instant at)
  {
    return Miles.Where(t =>
        t.ExpirationDate != null && 
        t.ExpirationDate > at || 
        t.CantExpire)
      .Select(t => t.GetMilesAmount(at)).Sum();
  }

  public void Remove(int miles, Instant when, Func<List<AwardedMiles>, List<AwardedMiles>> sort)
  {
    if (CalculateBalance(when) >= miles && Active)
    {
      var milesList = Miles.ToList();
      milesList = sort(milesList);

      foreach (var iter in milesList)
      {
        if (miles <= 0)
        {
          break;
        }

        if (iter.CantExpire || iter.ExpirationDate > when)
        {
          var milesAmount = iter.GetMilesAmount(when);
          if (milesAmount <= miles)
          {
            miles -= milesAmount.Value;
            iter.RemoveAll(when);
          }
          else
          {
            iter.Subtract(miles, when);
            miles = 0;
          }
        }
      }
    }
    else
    {
      throw new ArgumentException("Insufficient miles, id = " + ClientId + ", miles requested = " + miles);
    }
  }

  public void MoveMilesTo(AwardsAccount accountTo, int amount, Instant when)
  {
    if (CalculateBalance(when) >= amount && Active)
    {
      foreach (var iter in Miles)
      {
        if (iter.CantExpire || iter.ExpirationDate > when)
        {
          var milesAmount = iter.GetMilesAmount(when);
          if (milesAmount <= amount)
          {
            iter.TransferTo(accountTo);
            amount -= milesAmount.Value;
          }
          else
          {
            iter.Subtract(amount, when);
            iter.TransferTo(accountTo);
            amount -= iter.GetMilesAmount(when).Value;
          }
        }
      }

      Transactions++;
      accountTo.Transactions++;
    }
  }

  public void Activate() 
  {
    Active = true;
  }

  public void Deactivate() 
  {
    Active = false;
  }

  public IReadOnlyList<AwardedMiles> GetMiles() 
  {
    return Miles.ToList();
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