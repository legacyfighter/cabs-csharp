using LegacyFighter.Cabs.Entity;
using NodaTime;

namespace LegacyFighter.Cabs.Dto;

public class DriverPositionDtoV2
{
  public DriverPositionDtoV2()
  {

  }

  public DriverPositionDtoV2(Driver driver, double latitude, double longitude, Instant seenAt)
  {
    Driver = driver;
    Latitude = latitude;
    Longitude = longitude;
    SeenAt = seenAt;
  }

  public Driver Driver { get; set; }
  public double Latitude { get; set; }
  public double Longitude { get; set; }
  public Instant SeenAt { get; set; }
}