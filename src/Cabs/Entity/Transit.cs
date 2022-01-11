using System.Globalization;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.MoneyValue;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class Transit : BaseEntity
{


  public Transit()
  {
  }

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
  public Instant? Date { get; private set; }
  public Tariff Tariff { get; set; }
  private float _km;
  private Instant? _dateTime;

  public CarType.CarClasses? CarType { get; set; }
  public virtual Driver Driver { get; set; }

  public Money Price
  {
    get;
    set; //just for testing
  }

  public Statuses? Status { get; set; }

  public Instant? CompleteAt { get; private set; }

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

  public virtual Client Client { get; set; }

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
    set
    {
      Tariff = Tariff.OfTime(
        value.Value.InZone(DateTimeZoneProviders.Bcl.GetSystemDefault()).LocalDateTime);
      _dateTime = value;
    }
    get => _dateTime;
  }

  public Instant? Published { get; set; }

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

  public int AwaitingDriversResponses { get; set; } = 0;
  public virtual ISet<Driver> DriversRejections { get; set; } = new HashSet<Driver>();
  public virtual ISet<Driver> ProposedDrivers { get; set; } = new HashSet<Driver>();
  public Instant? AcceptedAt { get; set; }
  public Instant? Started { get; set; }
  public virtual Address From { get; set; }
  public virtual Address To { get; set; }

  public int PickupAddressChangeCounter { get; set; } = 0;

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

  public void CompleteTransitAt(Instant when)
  {
    CompleteAt = when;
  }

  public Money DriversFee { get; set; }

  public Money EstimatedPrice { get; private set; }
}