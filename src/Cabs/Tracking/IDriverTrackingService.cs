using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using NodaTime;

namespace LegacyFighter.Cabs.Tracking;

public interface IDriverTrackingService
{
  Task<DriverPosition> RegisterPosition(long? driverId, double latitude, double longitude, Instant seenAt);
  Task<Distance> CalculateTravelledDistance(long? driverId, Instant from, Instant to);

  public Task<List<DriverPositionDtoV2>> FindActiveDriversNearby(
    AddressDto address,
    Distance distance,
    List<CarClasses> carClasses);

  Task<List<DriverPositionDtoV2>> FindActiveDriversNearby(double latitudeMin,
    double latitudeMax,
    double longitudeMin,
    double longitudeMax,
    double latitude,
    double longitude,
    List<CarClasses> carClasses);
}