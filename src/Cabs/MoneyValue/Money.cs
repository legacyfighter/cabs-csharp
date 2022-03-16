using System.Globalization;

namespace LegacyFighter.Cabs.MoneyValue;

public class Money : IEquatable<Money>
{
  public static readonly Money Zero = new(0);
  public int IntValue { get; }

  protected Money()
  {
  }

  public Money(int value)
  {
    IntValue = value;
  }

  public static Money operator +(Money money, Money other)
  {
    return new Money(money.IntValue + other.IntValue);
  }

  public static Money operator -(Money money, Money other)
  {
    return new Money(money.IntValue - other.IntValue);
  }

  public Money Percentage(int percentage)
  {
    return new Money((int)Math.Round(percentage * IntValue / 100.0));
  }

  public Money Percentage(double percentage)
  {
    return new Money((int)Math.Round(percentage * IntValue / 100));
  }

  public bool Equals(Money other)
  {
    if (ReferenceEquals(null, other)) return false;
    if (ReferenceEquals(this, other)) return true;
    return IntValue == other.IntValue;
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj as Money == null) return false;
    return Equals((Money)obj);
  }

  public override int GetHashCode()
  {
    return IntValue;
  }

  public static bool operator ==(Money left, Money right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(Money left, Money right)
  {
    return !Equals(left, right);
  }

  public override string ToString()
  {
    var value = (double)IntValue / 100;
    return value.ToString("0.00", CultureInfo.CreateSpecificCulture("en-US"));
  }
}