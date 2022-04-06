using NodaTime;

namespace LegacyFighter.Cabs.Loyalty;

internal class ConstantUntil : IMiles, IEquatable<ConstantUntil>
{
  public static ConstantUntil Forever(int amount)
  {
    return new ConstantUntil(amount, Instant.MaxValue);
  }

  public static ConstantUntil Value(int amount, Instant when)
  {
    return new ConstantUntil(amount, when);
  }

  private readonly int? _amount;
  private readonly Instant _whenExpires;

  public ConstantUntil()
  {
  }

  public ConstantUntil(int? amount, Instant whenExpires)
  {
    _amount = amount;
    _whenExpires = whenExpires;
  }

  public int? GetAmountFor(Instant moment)
  {
    return _whenExpires >= moment ? _amount : 0;
  }

  public IMiles Subtract(int? amount, Instant moment)
  {
    if (GetAmountFor(moment) < amount)
    {
      throw new ArgumentException("Insufficient amount of miles");
    }

    return new ConstantUntil(_amount - amount, _whenExpires);
  }

  public Instant ExpiresAt()
  {
    return _whenExpires;
  }

  public bool Equals(ConstantUntil other)
  {
    if (ReferenceEquals(null, other)) return false;
    if (ReferenceEquals(this, other)) return true;
    return _amount == other._amount && _whenExpires.Equals(other._whenExpires);
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != GetType()) return false;
    return Equals((ConstantUntil)obj);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(_amount, _whenExpires);
  }

  public static bool operator ==(ConstantUntil left, ConstantUntil right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(ConstantUntil left, ConstantUntil right)
  {
    return !Equals(left, right);
  }
}