using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Pricing;

namespace LegacyFighter.Cabs.Ride;

public class RequestForTransit : BaseEntity
{
  public Guid RequestGuid { get; } = Guid.NewGuid();
  public Tariff Tariff { get; }
  public Distance Distance { get; }

  protected RequestForTransit()
  {
  }

  public RequestForTransit(Tariff tariff, Distance distance)
  {
    Tariff = tariff;
    Distance = distance;
  }

  public Money EstimatedPrice => Tariff.CalculateCost(Distance);

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as RequestForTransit)?.Id;
  }

  public static bool operator ==(RequestForTransit left, RequestForTransit right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(RequestForTransit left, RequestForTransit right)
  {
    return !Equals(left, right);
  }
}