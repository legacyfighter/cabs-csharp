using System.Globalization;
using LegacyFighter.Cabs.Entity;
using NodaTime;

namespace LegacyFighter.Cabs.Dto;

public class TransitDto
{
  public DriverDto Driver;

  public int? Factor;

  private float _distance;

  private string _distanceUnit;

  private decimal _baseFee;

  private Instant? _date;

  public TransitDto()
  {

  }

  public TransitDto(Transit transit)
  {
    Id = transit.Id;
    _distance = transit.Km;
    Factor = transit.Factor;
    if (transit.Price != null)
    {
      Price = new decimal(transit.Price.IntValue);
    }

    _date = transit.DateTime;
    Status = transit.Status;
    SetTariff(transit);
    foreach (var d in transit.ProposedDrivers) 
    {
      ProposedDrivers.Add(new DriverDto(d));
    }
    To = new AddressDto(transit.To);
    From = new AddressDto(transit.From);
    CarClass = transit.CarType;
    ClientDto = new ClientDto(transit.Client);
    if (transit.DriversFee != null)
    {
      DriverFee = new decimal(transit.DriversFee.IntValue);
    }

    if (transit.EstimatedPrice != null)
    {
      EstimatedPrice = new decimal(transit.EstimatedPrice.IntValue);
    }

    DateTime = transit.DateTime;
    Published = transit.Published;
    AcceptedAt = transit.AcceptedAt;
    Started = transit.Started;
    CompleteAt = transit.CompleteAt;

  }

  public float KmRate { get; private set; }

  private void SetTariff(Transit transit)
  {
    var day = _date.Value.InZone( DateTimeZoneProviders.Bcl.GetSystemDefault()).LocalDateTime;

    // wprowadzenie nowych cennikow od 1.01.2019
    if (day.Year <= 2018)
    {
      KmRate = 1.0f;
      Tariff = "Standard";
      return;
    }

    var year = day.Year;
    var leap = ((year % 4 == 0) && (year % 100 != 0)) || (year % 400 == 0);

    if (((leap && day.DayOfYear == 366) || (!leap && day.DayOfYear == 365)) ||
        (day.DayOfYear == 1 && day.Hour <= 6))
    {
      Tariff = "Sylwester";
      KmRate = 3.50f;
    }
    else
    {
      switch (day.DayOfWeek)
      {
        case IsoDayOfWeek.Monday:
        case IsoDayOfWeek.Tuesday:
        case IsoDayOfWeek.Wednesday:
        case IsoDayOfWeek.Thursday:
          KmRate = 1.0f;
          Tariff = "Standard";
          break;
        case IsoDayOfWeek.Friday:
          if (day.Hour < 17)
          {
            Tariff = "Standard";
            KmRate = 1.0f;
          }
          else
          {
            Tariff = "Weekend+";
            KmRate = 2.50f;
          }

          break;
        case IsoDayOfWeek.Saturday:
          if (day.Hour < 6 || day.Hour >= 17)
          {
            KmRate = 2.50f;
            Tariff = "Weekend+";
          }
          else if (day.Hour < 17)
          {
            KmRate = 1.5f;
            Tariff = "Weekend";
          }

          break;
        case IsoDayOfWeek.Sunday:
          if (day.Hour < 6)
          {
            KmRate = 2.50f;
            Tariff = "Weekend+";
          }
          else
          {
            KmRate = 1.5f;
            Tariff = "Weekend";
          }

          break;
      }
    }

  }

  public string Tariff { get; private set; }

  public string GetDistance(string unit)
  {
    var usCulture = CultureInfo.CreateSpecificCulture("en-US");
    _distanceUnit = unit;
    if (unit == "km")
    {
      if (_distance == Math.Ceiling(_distance))
      {
        return Math.Round(_distance).ToString(usCulture) + "km";

      }

      return _distance.ToString("0.000", usCulture) + "km";
    }

    if (unit == "miles")
    {
      var distance = _distance / 1.609344f;
      if (distance == Math.Ceiling(distance))
      {
        return Math.Round(distance).ToString(usCulture) + "miles";
      }

      return distance.ToString("0.000", usCulture) + "miles";
    }

    if (unit == "m")
    {
      return Math.Round(_distance*1000).ToString(usCulture) + "m";
    }

    throw new ArgumentException("Invalid unit " + unit);
  }

  public List<DriverDto> ProposedDrivers { get; set; } = new();
  public ClaimDto ClaimDto { get; set; }
  public AddressDto To { get; set; }
  public AddressDto From { get; set; }
  public CarType.CarClasses? CarClass { get; set; }
  public ClientDto ClientDto { get; set; }
  public long? Id { get; }
  public Transit.Statuses? Status { get; set; }
  public decimal? Price { get; }
  public decimal? DriverFee { get; set; }
  public Instant? DateTime { get; set; }
  public Instant? Published { get; set; }
  public Instant? AcceptedAt { get; set; }
  public Instant? Started { get; set; }
  public Instant? CompleteAt { get; set; }
  public decimal EstimatedPrice { get; set; }
}