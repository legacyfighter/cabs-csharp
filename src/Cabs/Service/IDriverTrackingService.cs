using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Entity;
using NodaTime;

namespace LegacyFighter.Cabs.Service;

public interface IDriverTrackingService
{
  Task<DriverPosition> RegisterPosition(long? driverId, double latitude, double longitude);
  Task<Distance> CalculateTravelledDistance(long? driverId, Instant from, Instant to);
}