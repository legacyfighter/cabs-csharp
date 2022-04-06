using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Pricing;

namespace LegacyFighter.Cabs.Ride;

public class Transit : BaseEntity
{
  public enum Statuses
  {
    InTransit,
    Completed
  }

  public Guid RequestGuid { get; private set; }
  public Statuses? Status { get; private set; }
  public Tariff Tariff { get; set; }
  private float _km;

  public Transit()
  {
    Status = Statuses.InTransit;
  }

  public Transit(long? id)
  {
    Id = id;
  }

  public Transit(Tariff tariff, Guid transitRequestGuid) 
  : this(Statuses.InTransit, tariff, transitRequestGuid)
  {
    
  }

  public Transit(Statuses status, Tariff tariff, Guid requestGuid)
  {
    Status = status;
    Tariff = tariff;
    RequestGuid = requestGuid;
  }

  public void ChangeDestination(Distance newDistance)
  {
    if (Status == Statuses.Completed)
    {
      throw new InvalidOperationException($"Address 'to' cannot be changed, id = {Id}");
    }

    _km = newDistance.ToKmInFloat();
  }

  public Money CompleteTransitAt(Distance distance)
  {
    if (Status == Statuses.InTransit)
    {
      _km = distance.ToKmInFloat();
      Status = Statuses.Completed;
      return CalculateFinalCosts();
    }
    else
    {
      throw new ArgumentException($"Cannot complete Transit, id = {Id}");
    }
  }

  public Money CalculateFinalCosts()
  {
    if (Status == Statuses.Completed)
    {
      return CalculateCost();
    }
    else
    {
      throw new InvalidOperationException("Cannot calculate final cost if the transit is not completed");
    }
  }

  private Money CalculateCost()
  {
    var money = Tariff.CalculateCost(Distance.OfKm(_km));
    return money;
  }

  public Distance Distance => Distance.OfKm(_km);

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as Transit)?.Id;
  }

  public static bool operator ==(Transit left, Transit right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(Transit left, Transit right)
  {
    return !Equals(left, right);
  }
}