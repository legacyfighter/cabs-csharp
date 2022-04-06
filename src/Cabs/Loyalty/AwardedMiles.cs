using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Loyalty;

public class AwardedMiles : BaseEntity
{
  public AwardedMiles()
  {
  }

  public AwardedMiles(AwardsAccount awardsAccount, long? transitId, long? clientId, Instant when, IMiles constantUntil) 
  {
    Account = awardsAccount;
    TransitId = transitId;
    ClientId = clientId;
    Date = when;
    Miles = constantUntil;
  }

  public void TransferTo(AwardsAccount account) 
  {
    ClientId = account.ClientId;
    Account = account;
  }

  public long? ClientId { get; protected set; }

  private string MilesJson { get; set; }

  public IMiles Miles
  {
    get => MilesJsonMapper.Deserialize(MilesJson);
    private set => MilesJson = MilesJsonMapper.Serialize(value);
  }

  public int? GetMilesAmount(Instant when) 
  {
    return Miles.GetAmountFor(when);
  }

  public Instant Date { get; } = SystemClock.Instance.GetCurrentInstant();

  public Instant? ExpirationDate => Miles.ExpiresAt();

  public bool CantExpire => ExpirationDate.Value.ToUnixTimeTicks() == Instant.MaxValue.ToUnixTimeTicks();

  public long? TransitId { get; }

  protected virtual AwardsAccount Account { get; set; }

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