using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Geolocation;
using NodaTime;

namespace LegacyFighter.Cabs.Tracking;

public class TransactionalDriverTrackingService : IDriverTrackingService
{
  private readonly IDriverTrackingService _inner;
  private readonly ITransactions _transactions;

  public TransactionalDriverTrackingService(IDriverTrackingService inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }
  
  public async Task<DriverPosition> RegisterPosition(long? driverId, double latitude, double longitude, Instant seenAt)
  {
    await using var tx = await _transactions.BeginTransaction();
    var position = await _inner.RegisterPosition(driverId, latitude, longitude, seenAt);
    await tx.Commit();
    return position;
  }

  public async Task<Distance> CalculateTravelledDistance(long? driverId, Instant from, Instant to)
  {
    return await _inner.CalculateTravelledDistance(driverId, from, to);
  }

  public async Task<List<DriverPositionDtoV2>> FindActiveDriversNearby(double latitudeMin,
    double latitudeMax,
    double longitudeMin,
    double longitudeMax,
    double latitude,
    double longitude,
    List<CarClasses> carClasses)
  {
    return await _inner.FindActiveDriversNearby(
      latitudeMin,
      latitudeMax,
      longitudeMin,
      longitudeMax,
      latitude,
      longitude,
      carClasses);
  }
}