using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;
using NodaTime;

namespace LegacyFighter.Cabs.TransitDetail;

public class TransitDetailsDto
{
  public long? TransitId { get; set; }
  public Instant? DateTime { get; set; }
  public Instant? CompletedAt { get; set; }
  public Instant? Started { get; set; }
  public Instant? AcceptedAt { get; set; }
  public Instant? PublishedAt { get; set; }
  public ClientDto Client { get; set; }
  public CarClasses? CarType { get; set; }
  public AddressDto From { get; set; }
  public AddressDto To { get; set; }
  public Money Price { get; set; }
  public Money DriverFee { get; set; }
  public long? DriverId { get; set; }
  public Money EstimatedPrice { get; set; }
  public Transit.Statuses Status { get; set; }
  public Distance Distance { get; set; }
  public int? BaseFee { get; set; }
  public float? KmRate { get; set; }
  public string TariffName { get; set; }

  public TransitDetailsDto(TransitDetails td)
  {
    TransitId = td.TransitId;
    DateTime = td.DateTime;
    CompletedAt = td.CompleteAt;
    Client = new ClientDto(td.Client);
    CarType = td.CarType;
    From = new AddressDto(td.From);
    To = new AddressDto(td.To);
    Started = td.Started;
    AcceptedAt = td.AcceptedAt;
    DriverFee = td.DriversFee;
    Price = td.Price;
    DriverId = td.DriverId;
    EstimatedPrice = td.EstimatedPrice;
    Status = td.Status;
    PublishedAt = td.PublishedAt;
    Distance = td.Distance;
    BaseFee = td.BaseFee;
    KmRate = td.KmRate;
    TariffName = td.TariffName;
  }

  public TransitDetailsDto(
    long? transitId,
    Instant dateTime,
    Instant completedAt,
    ClientDto client,
    CarClasses? carType,
    AddressDto from,
    AddressDto to,
    Instant started,
    Instant acceptedAt,
    Distance distance,
    Tariff tariff)
  {
    TransitId = transitId;
    DateTime = dateTime;
    CompletedAt = completedAt;
    Client = client;
    CarType = carType;
    From = from;
    To = to;
    Started = started;
    AcceptedAt = acceptedAt;
    Distance = distance;
    KmRate = tariff.KmRate;
    BaseFee = tariff.BaseFee;
    TariffName = tariff.Name;
  }
}