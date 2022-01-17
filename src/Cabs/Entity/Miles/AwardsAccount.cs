using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity.Miles;

public class AwardsAccount : BaseEntity
{
  public static AwardsAccount NotActiveAccount(Client client, Instant date) 
  {
    return new AwardsAccount(client, false, date);
  }

  public AwardsAccount()
  {
  }

  public AwardsAccount(Client client, bool isActive, Instant date) 
  {
    Client = client;
    Active = isActive;
    Date = date;
  }

  public virtual Client Client { get; }
  public Instant Date { get; } = SystemClock.Instance.GetCurrentInstant();
  public bool Active { private set; get; } = false;
  public int Transactions { get; private set; } = 0;

  protected virtual ISet<AwardedMiles> Miles { get; set; } = new HashSet<AwardedMiles>();

  public AwardedMiles AddExpiringMiles(int amount, Instant expireAt, Transit transit, Instant when) 
  {
    var expiringMiles = new AwardedMiles(this, transit, Client, when, ConstantUntil.Value(amount, expireAt));
    Miles.Add(expiringMiles);
    Transactions++;
    return expiringMiles;
  }

  public AwardedMiles AddNonExpiringMiles(int amount, Instant when)
  {
    var nonExpiringMiles = new AwardedMiles(this, null, Client, when, ConstantUntil.Forever(amount));
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

  public void Remove(int miles, Instant when, int transitsCounter, int claimsCounter, Client.Types? type, bool isSunday)
  {
    if (CalculateBalance(when) >= miles && Active)
    {
      var milesList = Miles.ToList();
      if (claimsCounter >= 3)
      {
        milesList = milesList.OrderBy(m => m.ExpirationDate.HasValue)
          .ThenByDescending(m => m.ExpirationDate).ToList();
      }
      else if (type == Client.Types.Vip)
      {
        milesList = milesList.OrderBy(m => m.CantExpire).ThenBy(m => m.ExpirationDate).ToList();
      }
      else if (transitsCounter >= 15 && isSunday)
      {
        milesList = milesList.OrderBy(m => m.CantExpire).ThenBy(m => m.ExpirationDate).ToList();
      }
      else if (transitsCounter >= 15)
      {
        milesList = milesList.OrderBy(m => m.CantExpire).ThenBy(m => m.Date).ToList();
      }
      else
      {
        milesList = milesList.OrderBy(m => m.Date).ToList();
      }

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
      throw new ArgumentException("Insufficient miles, id = " + Client.Id + ", miles requested = " + miles);
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