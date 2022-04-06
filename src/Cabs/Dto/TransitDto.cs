using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Crm.Claims;
using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.TransitDetail;
using NodaTime;

namespace LegacyFighter.Cabs.Dto;

public class TransitDto
{
  public DriverDto Driver { get; set; }
  public int? Factor { get; set; }
  private readonly Distance _distance;
  private string _distanceUnit;
  private decimal _baseFee;
  private Instant? _date;

  public TransitDto()
  {

  }

  public TransitDto(Transit transit, TransitDetailsDto transitDetails)

    : this(transitDetails.TransitId, transitDetails.TariffName,
      transitDetails.Status, 
      transit.Driver == null ? null : new DriverDto(transit.Driver),
      transitDetails.Distance, 
      transitDetails.KmRate.Value,
      transitDetails.Price != null ? new decimal(transitDetails.Price.IntValue) : null,
      transitDetails.DriverFee != null ? new decimal(transitDetails.DriverFee.IntValue) : null,
      transitDetails.EstimatedPrice != null ? new decimal(transitDetails.EstimatedPrice.IntValue) : null,
      new decimal(transitDetails.BaseFee.Value),
      transitDetails.DateTime, 
      transitDetails.PublishedAt,
      transitDetails.AcceptedAt, 
      transitDetails.Started, 
      transitDetails.CompletedAt,
      null, 
      new List<DriverDto>(), 
      transitDetails.From,
      transitDetails.To, 
      transitDetails.CarType, 
      transitDetails.Client)
  {
    foreach (var d in transit.ProposedDrivers)
    {
      ProposedDrivers.Add(new DriverDto(d));
    }
  }

  public TransitDto(
    long? id,
    string tariff,
    Transit.Statuses? status,
    DriverDto driver,
    Distance distance,
    float kmRate,
    decimal? price,
    decimal? driverFee,
    decimal? estimatedPrice,
    decimal baseFee,
    Instant? dateTime,
    Instant? published,
    Instant? acceptedAt,
    Instant? started,
    Instant? completeAt,
    ClaimDto claimDto,
    List<DriverDto> proposedDrivers,
    AddressDto from,
    AddressDto to,
    CarClasses? carClass,
    ClientDto clientDto)
  {
    Id = id;
    Factor = 1;
    Tariff = tariff;
    Status = status;
    Driver = driver;
    _distance = distance;
    KmRate = kmRate;
    Price = price;
    DriverFee = driverFee;
    EstimatedPrice = estimatedPrice;
    _baseFee = baseFee;
    DateTime = dateTime;
    Published = published;
    AcceptedAt = acceptedAt;
    Started = started;
    CompleteAt = completeAt;
    ClaimDto = claimDto;
    ProposedDrivers = proposedDrivers;
    To = to;
    From = from;
    CarClass = carClass;
    ClientDto = clientDto;
  }

  public float KmRate { get; private set; }
  public string Tariff { get; private set; }

  public string GetDistance(string unit)
  {
    _distanceUnit = unit;
    return _distance.PrintIn(unit);
  }

  public List<DriverDto> ProposedDrivers { get; set; } = new();
  public ClaimDto ClaimDto { get; set; }
  public AddressDto To { get; set; }
  public AddressDto From { get; set; }
  public CarClasses? CarClass { get; set; }
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
  public decimal? EstimatedPrice { get; set; }
}