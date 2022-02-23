using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using NodaTime;

namespace LegacyFighter.Cabs.Service;

public class DriverTrackingService : IDriverTrackingService
{
  private readonly IDriverPositionRepository _positionRepository;
  private readonly IDriverRepository _driverRepository;
  private readonly DistanceCalculator _distanceCalculator;
  private IClock _clock;

  public DriverTrackingService(IDriverPositionRepository positionRepository, IDriverRepository driverRepository, DistanceCalculator distanceCalculator, IClock clock)
  {
    _positionRepository = positionRepository;
    _driverRepository = driverRepository;
    _distanceCalculator = distanceCalculator;
    _clock = clock;
  }

  public async Task<DriverPosition> RegisterPosition(long? driverId, double latitude, double longitude)
  {
    var driver = await _driverRepository.Find(driverId);
    if (driver == null)
    {
      throw new ArgumentException("Driver does not exists, id = " + driverId);
    }

    if (driver.Status != Driver.Statuses.Active)
    {
      throw new InvalidOperationException("Driver is not active, cannot register position, id = " + driverId);
    }

    var position = new DriverPosition
    {
      Driver = driver,
      SeenAt = _clock.GetCurrentInstant(),
      Latitude = latitude,
      Longitude = longitude
    };
    return await _positionRepository.Save(position);
  }

  public async Task<Distance> CalculateTravelledDistance(long? driverId, Instant @from, Instant to)
  {
    var driver = await _driverRepository.Find(driverId);
    if (driver == null)
    {
      throw new ArgumentException("Driver does not exists, id = " + driverId);
    }

    var positions =
      await _positionRepository.FindByDriverAndSeenAtBetweenOrderBySeenAtAsc(driver, @from, to);
    double distanceTravelled = 0;

    if (positions.Count > 1)
    {
      var previousPosition = positions[0];

      foreach (var position in positions.Skip(1)) 
      {
        distanceTravelled += _distanceCalculator.CalculateByGeo(
          previousPosition.Latitude,
          previousPosition.Longitude,
          position.Latitude,
          position.Longitude
        );

        previousPosition = position;
      }
    }

    return Distance.OfKm(distanceTravelled);
  }
}