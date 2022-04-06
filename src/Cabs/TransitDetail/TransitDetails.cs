using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;
using NodaTime;

namespace LegacyFighter.Cabs.TransitDetail;

public class TransitDetails : BaseEntity
{
  public Instant? DateTime { get; private set; }
  public Instant? CompleteAt { get; private set; }
  public virtual Client Client { get; private set; }
  public CarClasses? CarType { get; }
  public virtual Address From { get; private set; }
  public virtual Address To { get; private set; }
  public Instant? Started { get; private set; }
  public Instant? AcceptedAt { get; private set; }
  public Money DriversFee { get; private set; }
  public Money Price { get; private set; }
  public long? DriverId { get; private set; }
  public Money EstimatedPrice { get; private set; }
  public Transit.Statuses Status { get; private set; }
  public Instant? PublishedAt { get; private set; }
  public Distance Distance { get; private set; }
  public long? TransitId { get; private set; }
  private Tariff Tariff { get; set; }

  protected TransitDetails()
  {

  }

  public TransitDetails(
    Instant dateTime,
    long? transitId,
    Address from,
    Address to,
    Distance distance,
    Client client,
    CarClasses? carClass,
    Money estimatedPrice,
    Tariff tariff)
  {
    DateTime = dateTime;
    TransitId = transitId;
    From = from;
    To = to;
    Distance = distance;
    Client = client;
    CarType = carClass;
    Status = Transit.Statuses.Draft;
    EstimatedPrice = estimatedPrice;
    Tariff = tariff;
  }

  public void SetStartedAt(Instant when)
  {
    Started = when;
    Status = Transit.Statuses.InTransit;
  }

  public void SetAcceptedAt(Instant when, long? driverId)
  {
    AcceptedAt = when;
    DriverId = driverId;
    Status = Transit.Statuses.TransitToPassenger;
  }

  public void SetPublishedAt(Instant when)
  {
    PublishedAt = when;
    Status = Transit.Statuses.WaitingForDriverAssignment;
  }

  public void SetCompletedAt(Instant when, Money price, Money driverFee)
  {
    CompleteAt = when;
    Price = price;
    DriversFee = driverFee;
    Status = Transit.Statuses.Completed;
  }

  public void SetPickupChangedTo(Address newAddress, Distance newDistance)
  {
    From = newAddress;
    Distance = newDistance;
  }

  public void SetDestinationChangedTo(Address newAddress, Distance distance)
  {
    To = newAddress;
    Distance = distance;
  }
  
  public void SetAsCancelled()
  {
    Status = Transit.Statuses.Cancelled;
  }
  
  public float? KmRate
  {
    get
    {
      if (Tariff == null)
      {
        return null;
      }

      return Tariff.KmRate;
    }
  }

  public int? BaseFee
  {
    get
    {
      if (Tariff == null)
      {
        return null;
      }

      return Tariff.BaseFee;
    }
  }

  public string TariffName
  {
    get
    {
      if (Tariff == null)
      {
        return null;
      }

      return Tariff.Name;
    }
  }
}
