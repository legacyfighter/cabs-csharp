using System.Globalization;

namespace LegacyFighter.Cabs.MoneyValue;

public class Money : IEquatable<Money>
{
  public static readonly Money Zero = new(0);
  private readonly int _value;

  public Money(int value)
  {
    _value = value;
  }

  public static Money operator +(Money money, Money other)
  {
    return new Money(money._value + other._value);
  }

  public static Money operator -(Money money, Money other)
  {
    return new Money(money._value - other._value);
  }

  public Money Percentage(int percentage)
  {
    return new Money((int)Math.Round(percentage * _value / 100.0));
  }

  public int ToInt()
  {
    return _value;
  }

  public bool Equals(Money other)
  {
    if (ReferenceEquals(null, other)) return false;
    if (ReferenceEquals(this, other)) return true;
    return _value == other._value;
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != GetType()) return false;
    return Equals((Money)obj);
  }

  public override int GetHashCode()
  {
    return _value;
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
    var value = (double)_value / 100;
    return value.ToString("0.00", CultureInfo.CreateSpecificCulture("en-US"));
  }
}