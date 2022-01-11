using LegacyFighter.Cabs.Entity;
using NodaTime;

namespace LegacyFighter.Cabs.Dto;

public class DriverSessionDto
{
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
  public CarType.CarClasses? CarClass { get; set; }
}