using NodaTime;

namespace LegacyFighter.Cabs.Entity.Miles;

public class TwoStepExpiringMiles : IMiles, IEquatable<TwoStepExpiringMiles>
{
  private readonly int? _amount;
  private readonly Instant _whenFirstHalfExpires;
  private readonly Instant _whenExpires;

  public TwoStepExpiringMiles(int? amount)
  {
  }

  public TwoStepExpiringMiles(int? amount, Instant whenFirstHalfExpires, Instant whenExpires)
  {
    _amount = amount;
    _whenFirstHalfExpires = whenFirstHalfExpires;
    _whenExpires = whenExpires;
  }

  public int? GetAmountFor(Instant moment)
  {
    if (_whenFirstHalfExpires >= moment)
    {
      return _amount;
    }

    if (_whenExpires >= moment)
    {
      return _amount - HalfOf(_amount);
    }

    return 0;
  }

  private int? HalfOf(int? amount)
  {
    return amount / 2;
  }

  public IMiles Subtract(int? amount, Instant moment)
  {
    var currentAmount = GetAmountFor(moment);
    if (currentAmount < amount)
    {
      throw new ArgumentException("Insufficient amount of miles");
    }

    return new TwoStepExpiringMiles(currentAmount.Value - amount.Value, _whenFirstHalfExpires, _whenExpires);
  }

  public Instant ExpiresAt()
  {
    return _whenExpires;
  }

  public bool Equals(TwoStepExpiringMiles other)
  {
    if (ReferenceEquals(null, other)) return false;
    if (ReferenceEquals(this, other)) return true;
    return _amount == other._amount && _whenFirstHalfExpires.Equals(other._whenFirstHalfExpires) &&
           _whenExpires.Equals(other._whenExpires);
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != GetType()) return false;
    return Equals((TwoStepExpiringMiles)obj);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(_amount, _whenFirstHalfExpires, _whenExpires);
  }

  public static bool operator ==(TwoStepExpiringMiles left, TwoStepExpiringMiles right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(TwoStepExpiringMiles left, TwoStepExpiringMiles right)
  {
    return !Equals(left, right);
  }

  public override string ToString()
  {
    return "TwoStepExpiringMiles{" +
           "amount=" + _amount +
           ", whenFirstHalfExpires=" + _whenFirstHalfExpires +
           ", whenExpires=" + _whenExpires +
           '}';
  }
}
