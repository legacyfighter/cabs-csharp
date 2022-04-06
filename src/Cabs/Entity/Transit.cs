using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class Transit : BaseEntity
{

  public enum Statuses
  {
    Draft,
    Cancelled,
    WaitingForDriverAssignment,
    DriverAssignmentFailed,
    TransitToPassenger,
    InTransit,
    Completed
  }

  public enum DriverPaymentStatuses
  {
    NotPaid,
    Paid,
    Claimed,
    Returned
  }

  public enum ClientPaymentStatuses
  {
    NotPaid,
    Paid,
    Returned
  }

  private DriverPaymentStatuses? DriverPaymentStatus { get; set; }
  private ClientPaymentStatuses? ClientPaymentStatus { get; set; }
  private Client.PaymentTypes? PaymentType { get; set; }
  public Tariff Tariff { get; set; }
  private float _km;

  public Transit()
  {
  }

  public Transit(long? id)
  {
    Id = id;
  }

  public Transit(Instant when, Distance distance)
    : this(Statuses.Draft, when, distance)
  {
  }

  public Transit(Statuses status, Instant when, Distance distance)
  {
    DateTime = when;
    Km = distance.ToKmInFloat();
    Status = status;
  }

  public void ChangePickupTo(Address newAddress, Distance newDistance, double distanceFromPreviousPickup)
  {
    if (distanceFromPreviousPickup > 0.25)
    {
      throw new InvalidOperationException("Address 'from' cannot be changed, id = " + Id);
    }

    if (Status != Statuses.Draft &&
        Status != Statuses.WaitingForDriverAssignment)
    {
      throw new InvalidOperationException("Address 'from' cannot be changed, id = " + Id);
    }
    else if (PickupAddressChangeCounter > 2)
    {
      throw new InvalidOperationException("Address 'from' cannot be changed, id = " + Id);
    }

    PickupAddressChangeCounter = PickupAddressChangeCounter + 1;
    Km = newDistance.ToKmInFloat();
    EstimateCost();
  }

  public void ChangeDestinationTo(Address newAddress, Distance newDistance)
  {
    if (Status == Statuses.Completed)
    {
      throw new InvalidOperationException("Address 'to' cannot be changed, id = " + Id);
    }

    Km = newDistance.ToKmInFloat();
    EstimateCost();
  }

  public void Cancel()
  {
    if (!new HashSet<Statuses?> { Statuses.Draft, Statuses.WaitingForDriverAssignment, Statuses.TransitToPassenger }
          .Contains(Status))
    {
      throw new InvalidOperationException("Transit cannot be cancelled, id = " + Id);
    }

    Status = Statuses.Cancelled;
    Driver = null;
    Km = Distance.Zero.ToKmInFloat();
    AwaitingDriversResponses = 0;
  }

  public bool CanProposeTo(Driver driver)
  {
    return !DriversRejections.Contains(driver);
  }

  public void ProposeTo(Driver driver)
  {
    if (CanProposeTo(driver))
    {
      ProposedDrivers.Add(driver);
      AwaitingDriversResponses++;
    }
  }

  public void FailDriverAssignment()
  {
    Status = Statuses.DriverAssignmentFailed;
    Driver = null;
    Km = Distance.Zero.ToKmInFloat();
    AwaitingDriversResponses = 0;
  }

  public bool ShouldNotWaitForDriverAnyMore(Instant date)
  {
    return Status == Statuses.Cancelled || Published + Duration.FromSeconds(300) < date;
  }

  public void AcceptBy(Driver driver, Instant when)
  {
    if (Driver != null)
    {
      throw new InvalidOperationException("Transit already accepted, id = " + Id);
    }
    else
    {
      if (!ProposedDrivers.Contains(driver))
      {
        throw new InvalidOperationException("Driver out of possible drivers, id = " + Id);
      }
      else
      {
        if (DriversRejections.Contains(driver))
        {
          throw new InvalidOperationException("Driver out of possible drivers, id = " + Id);
        }
      }

      Driver = driver;
      driver.Occupied = true;
      AwaitingDriversResponses = 0;
      Status = Statuses.TransitToPassenger;
    }
  }

  public void Start(Instant when)
  {
    if (Status != Statuses.TransitToPassenger)
    {
      throw new InvalidOperationException("Transit cannot be started, id = " + Id);
    }

    Status = Statuses.InTransit;
  }

  public void RejectBy(Driver driver)
  {
    DriversRejections.Add(driver);
    AwaitingDriversResponses--;
  }

  public void PublishAt(Instant when)
  {
    Status = Statuses.WaitingForDriverAssignment;
    Published = when;
  }

  public void CompleteTransitAt(Instant when, Address destinationAddress, Distance distance)
  {
    if (Status == Statuses.InTransit)
    {
      Km = distance.ToKmInFloat();
      EstimateCost();
      Status = Statuses.Completed;
      CalculateFinalCosts();
    }
    else
    {
      throw new ArgumentException("Cannot complete Transit, id = " + Id);
    }
  }

  public virtual Driver Driver { get; protected set; }

  public Money Price
  {
    get;
    set; //just for testing
  }

  public Statuses? Status { get; private set; }

  public Money EstimateCost()
  {
    if (Status == Statuses.Completed)
    {
      throw new InvalidOperationException("Estimating cost for completed transit is forbidden, id = " + Id);
    }

    var estimated = CalculateCost();

    EstimatedPrice = estimated;
    Price = null;

    return estimated;
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
    Price = money;
    return money;
  }

  public Instant? DateTime
  {
    set =>
      Tariff = Tariff.OfTime(
        value.Value.InZone(DateTimeZoneProviders.Bcl.GetSystemDefault()).LocalDateTime);
  }

  public Distance KmDistance
  {
    get => Distance.OfKm(Km);
    set => Km = value.ToKmInFloat();
  }

  private float Km
  {
    get => _km;
    set
    {
      _km = value;
      EstimateCost();
    }
  }

  public int AwaitingDriversResponses { get; private set; } = 0;
  protected virtual ISet<Driver> DriversRejections { get; set; } = new HashSet<Driver>();
  public virtual ISet<Driver> ProposedDrivers { get; protected set; } = new HashSet<Driver>();
  private int PickupAddressChangeCounter { get; set; } = 0;

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

  public Money EstimatedPrice { get; private set; }
  public Instant? Published { get; private set; }
}