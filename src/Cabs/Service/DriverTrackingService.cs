using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.DriverReports.TravelledDistances;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using NodaTime;

namespace LegacyFighter.Cabs.Service;

public class DriverTrackingService : IDriverTrackingService
{
  private readonly IDriverPositionRepository _positionRepository;
  private readonly IDriverRepository _driverRepository;
  private readonly ITravelledDistanceService _travelledDistanceService;

  public DriverTrackingService(IDriverPositionRepository positionRepository, IDriverRepository driverRepository, ITravelledDistanceService travelledDistanceService)
  {
    _positionRepository = positionRepository;
    _driverRepository = driverRepository;
    _travelledDistanceService = travelledDistanceService;
  }

  public async Task<DriverPosition> RegisterPosition(long? driverId, double latitude, double longitude, Instant seenAt)
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
      SeenAt = seenAt,
      Latitude = latitude,
      Longitude = longitude
    };
    var driverPosition = await _positionRepository.Save(position);
    await _travelledDistanceService.AddPosition(position);
    return driverPosition;
  }

  public async Task<Distance> CalculateTravelledDistance(long? driverId, Instant from, Instant to)
  {
    var driver = await _driverRepository.Find(driverId);
    if (driver == null)
    {
      throw new ArgumentException("Driver does not exists, id = " + driverId);
    }

    return await _travelledDistanceService.CalculateDistance(driverId.Value, from, to);
  }
}