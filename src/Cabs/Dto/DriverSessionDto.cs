using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Entity;
using NodaTime;

namespace LegacyFighter.Cabs.Dto;

public class DriverSessionDto
{
  public DriverSessionDto(
    Instant loggedAt,
    Instant? loggedOutAt,
    string platesNumber,
    CarClasses carClass,
    string carBrand)
  {
    LoggedAt = loggedAt;
    LoggedOutAt = loggedOutAt;
    PlatesNumber = platesNumber;
    CarClass = carClass;
    CarBrand = carBrand;
  }

  public DriverSessionDto()
  {

  }

  public DriverSessionDto(DriverSession session)
  {
    CarBrand = session.CarBrand;
    PlatesNumber = session.PlatesNumber;
    LoggedAt = session.LoggedAt;
    LoggedOutAt = session.LoggedOutAt;
    CarClass = session.CarClass;
  }

  public string CarBrand { get; set; }
  public Instant LoggedAt { get; set; }
  public Instant? LoggedOutAt { get; set; }
  public string PlatesNumber { get; set; }
  public CarClasses? CarClass { get; set; }
}