using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.DriverFleet.DriverReports.TravelledDistances;
using LegacyFighter.Cabs.Geolocation;
using NodaTime;

namespace LegacyFighter.Cabs.Tracking;

public class DriverTrackingService : IDriverTrackingService
{
  private readonly IDriverPositionRepository _positionRepository;
  private readonly IDriverService _driverService;
  private readonly IDriverSessionService _driverSessionService;
  private readonly ITravelledDistanceService _travelledDistanceService;
  private readonly IClock _clock;

  public DriverTrackingService(
    IDriverPositionRepository positionRepository, 
    IDriverService driverService, 
    IDriverSessionService driverSessionService, 
    ITravelledDistanceService travelledDistanceService, 
    IClock clock)
  {
    _positionRepository = positionRepository;
    _driverService = driverService;
    _driverSessionService = driverSessionService;
    _travelledDistanceService = travelledDistanceService;
    _clock = clock;
  }

  public async Task<DriverPosition> RegisterPosition(long? driverId, double latitude, double longitude, Instant seenAt)
  {
    var driver = await _driverService.LoadDriver(driverId);
    if (driver.Status != Driver.Statuses.Active) {
      throw new InvalidOperationException($"Driver is not active, cannot register position, id = {driverId}");
    }
    var position = new DriverPosition
    {
      DriverId = driverId,
      SeenAt = seenAt,
      Latitude = latitude,
      Longitude = longitude
    };
    position = await _positionRepository.Save(position);
    await _travelledDistanceService.AddPosition(driverId, latitude, longitude, seenAt);
    return position;
  }

  public async Task<Distance> CalculateTravelledDistance(long? driverId, Instant from, Instant to)
  {
    return await _travelledDistanceService.CalculateDistance(driverId.Value, from, to);
  }

  public async Task<List<DriverPositionDtoV2>> FindActiveDriversNearby(double latitudeMin,
    double latitudeMax,
    double longitudeMin,
    double longitudeMax,
    double latitude,
    double longitude,
    List<CarClasses> carClasses)
  {
    var driversAvgPositions = await _positionRepository
      .FindAverageDriverPositionSince(latitudeMin, latitudeMax, longitudeMin, longitudeMax,
        _clock.GetCurrentInstant().Minus(Duration.FromMinutes(5)));

    driversAvgPositions.Sort((d1, d2) =>
      Math.Sqrt(Math.Pow(latitude - d1.Latitude, 2) + Math.Pow(longitude - d1.Longitude, 2)).CompareTo(
        Math.Sqrt(Math.Pow(latitude - d2.Latitude, 2) + Math.Pow(longitude - d2.Longitude, 2))
      ));
    driversAvgPositions = driversAvgPositions.Take(20).ToList();

    var driversIds = driversAvgPositions.Select(p => p.DriverId).ToList();
    var activeDriverIdsInSpecificCar = await _driverSessionService.FindCurrentlyLoggedDriverIds(driversIds, carClasses);

    driversAvgPositions = driversAvgPositions
      .Where(dp => activeDriverIdsInSpecificCar.Contains(dp.DriverId)).ToList();

    var drivers = (await _driverService.LoadDrivers(driversIds))
      .ToDictionary(d => d.Id, d => d);

    driversAvgPositions = driversAvgPositions.Where(dap =>
    {
      var d = drivers[dap.DriverId];
      return d.Status == Driver.Statuses.Active && !d.IsOccupied;
    }).ToList();
    return driversAvgPositions;
  }
}