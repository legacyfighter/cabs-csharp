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
  public int? Factor { get; set; }
  private float _km;
  public const int BaseFee = 8;

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
    var baseFee = BaseFee;
    var factorToCalculate = Factor;
    if (factorToCalculate == null)
    {
      factorToCalculate = 1;
    }

    float kmRate;
    var day = DateTime.Value.InZone(DateTimeZoneProviders.Bcl.GetSystemDefault()).LocalDateTime;
    // wprowadzenie nowych cennikow od 1.01.2019
    if (day.Year <= 2018)
    {
      kmRate = 1.0f;
      baseFee++;
    }
    else
    {
      if ((day.Month == 12 && day.Day == 31) ||
          (day.Month == 1 && day.Day == 1 && day.Hour <= 6))
      {
        kmRate = 3.50f;
        baseFee += 3;
      }
      else
      {
        // piątek i sobota po 17 do 6 następnego dnia
        if ((day.DayOfWeek == IsoDayOfWeek.Friday && day.Hour >= 17) ||
            (day.DayOfWeek == IsoDayOfWeek.Saturday && day.Hour <= 6) ||
            (day.DayOfWeek == IsoDayOfWeek.Saturday && day.Hour >= 17) ||
            (day.DayOfWeek == IsoDayOfWeek.Sunday && day.Hour <= 6))
        {
          kmRate = 2.50f;
          baseFee += 2;
        }
        else
        {
          // pozostałe godziny weekendu
          if ((day.DayOfWeek == IsoDayOfWeek.Saturday && day.Hour > 6 && day.Hour < 17) ||
              (day.DayOfWeek == IsoDayOfWeek.Sunday && day.Hour > 6))
          {
            kmRate = 1.5f;
          }
          else
          {
            // tydzień roboczy
            kmRate = 1.0f;
            baseFee++;
          }
        }
      }
    }

    var pricedecimal = new decimal(_km * kmRate * factorToCalculate.Value + baseFee);
    pricedecimal = decimal.Round(pricedecimal, 2, MidpointRounding.ToPositiveInfinity);
    var finalPrice = new Money(int.Parse(pricedecimal.ToString("0.00", CultureInfo.InvariantCulture).Replace(".", "")));
    Price = finalPrice;
    return finalPrice;
  }

  public Instant? DateTime { set; get; }

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