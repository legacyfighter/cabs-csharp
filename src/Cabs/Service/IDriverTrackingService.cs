using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Geolocation;
using NodaTime;

namespace LegacyFighter.Cabs.Service;

public interface IDriverTrackingService
{
  Task<DriverPosition> RegisterPosition(long? driverId, double latitude, double longitude, Instant seenAt);
  Task<Distance> CalculateTravelledDistance(long? driverId, Instant from, Instant to);
}