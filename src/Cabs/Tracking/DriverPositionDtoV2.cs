using NodaTime;

namespace LegacyFighter.Cabs.Tracking;

public class DriverPositionDtoV2
{
  public DriverPositionDtoV2()
  {

  }

  public DriverPositionDtoV2(long? driverId, double latitude, double longitude, Instant seenAt)
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