using LegacyFighter.Cabs.Assignment;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Pricing;
using NodaTime;

namespace LegacyFighter.Cabs.Ride.Details;

public class TransitDetails : BaseEntity
{
  internal Instant? DateTime { get; private set; }
  internal Instant? CompleteAt { get; private set; }
  internal virtual Client Client { get; private set; }
  internal CarClasses? CarType { get; }
  internal virtual Address From { get; private set; }
  internal virtual Address To { get; private set; }
  internal Instant? Started { get; private set; }
  internal Instant? AcceptedAt { get; private set; }
  internal Money DriversFee { get; private set; }
  internal Money Price { get; private set; }
  internal long? DriverId { get; private set; }
  internal Money EstimatedPrice { get; private set; }
  internal Statuses Status { get; private set; }
  internal Instant? PublishedAt { get; private set; }
  internal Distance Distance { get; private set; }
  public long? TransitId { get; private set; }
  internal Guid RequestGuid { get; private set; }
  private Tariff Tariff { get; set; }

  protected TransitDetails()
  {

  }

  public TransitDetails(
    Instant dateTime,
    Guid requestGuid,
    Address from,
    Address to,
    Distance distance,
    Client client,
    CarClasses? carClass,
    Money estimatedPrice,
    Tariff tariff)
  {
    RequestGuid = requestGuid;
    DateTime = dateTime;
    From = from;
    To = to;
    Distance = distance;
    Client = client;
    CarType = carClass;
    Status = Statuses.Draft;
    EstimatedPrice = estimatedPrice;
    Tariff = tariff;
  }

  internal void SetStartedAt(Instant when, long? transitId)
  {
    Started = when;
    Status = Statuses.InTransit;
    TransitId = transitId;
  }

  internal void SetAcceptedAt(Instant when, long? driverId)
  {
    AcceptedAt = when;
    DriverId = driverId;
    Status = Statuses.TransitToPassenger;
  }

  internal void SetPublishedAt(Instant when)
  {
    PublishedAt = when;
    Status = Statuses.WaitingForDriverAssignment;
  }

  internal void SetCompletedAt(Instant when, Money price, Money driverFee)
  {
    CompleteAt = when;
    Price = price;
    DriversFee = driverFee;
    Status = Statuses.Completed;
  }

  internal void SetPickupChangedTo(Address newAddress, Distance newDistance)
  {
    From = newAddress;
    Distance = newDistance;
  }

  internal void SetDestinationChangedTo(Address newAddress, Distance newDistance)
  {
    To = newAddress;
    Distance = newDistance;
  }

  internal void InvolvedDriversAre(InvolvedDriversSummary involvedDriversSummary)
  {
    if (involvedDriversSummary.Status == AssignmentStatuses.DriverAssignmentFailed)
    {
      Status = Statuses.DriverAssignmentFailed;
    }
    else
    {
      Status = Statuses.TransitToPassenger;
    }
  }

  internal void SetAsCancelled()
  {
    Status = Statuses.Cancelled;
  }

  internal float? KmRate
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

  internal int? BaseFee
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

  internal string TariffName
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
