using NodaTime;

namespace LegacyFighter.Cabs.Dto;

public class DriverPositionDto
{

  public DriverPositionDto()
  {

  }

  public DriverPositionDto(long? driverId, double latitude, double longitude, Instant seenAt)
  {
    DriverId = driverId;
    Latitude = latitude;
    Longitude = longitude;
    SeenAt = seenAt;
  }

  public long? DriverId { get; set; }
  public double Latitude { get; set; }
  public double Longitude { get; set; }
  public Instant SeenAt { get; set; }
}