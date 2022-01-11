using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.MoneyValue;

namespace LegacyFighter.Cabs.Entity;

public class DriverFee : BaseEntity
{

  public enum FeeTypes
  {
    Flat,
    Percentage
  }

  public DriverFee()
  {

  }

  public DriverFee(FeeTypes feeType, Driver driver, int amount, int min)
  {
    FeeType = feeType;
    Driver = driver;
    Amount = amount;
    Min = new Money(min);
  }

  public FeeTypes FeeType { get; set; }
  public virtual Driver Driver { get; set; }
  public int Amount { get; set; }
  public Money Min { get; set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as DriverFee)?.Id;
  }

  public static bool operator ==(DriverFee left, DriverFee right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(DriverFee left, DriverFee right)
  {
    return !Equals(left, right);
  }
}