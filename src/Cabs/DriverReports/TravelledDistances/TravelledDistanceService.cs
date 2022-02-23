using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Service;
using NodaTime;

namespace LegacyFighter.Cabs.DriverReports.TravelledDistances;

public interface ITravelledDistanceService
{
  Task<Distance> CalculateDistance(long driverId, Instant from, Instant to);
  Task AddPosition(DriverPosition driverPosition);
}

public class TravelledDistanceService : ITravelledDistanceService
{
  private readonly IClock _clock;
  private readonly ITravelledDistanceRepository _travelledDistanceRepository;
  private readonly DistanceCalculator _distanceCalculator;

  public TravelledDistanceService(IClock clock, ITravelledDistanceRepository travelledDistanceRepository,
    DistanceCalculator distanceCalculator)
  {
    _clock = clock;
    _travelledDistanceRepository = travelledDistanceRepository;
    _distanceCalculator = distanceCalculator;
  }

  public async Task<Distance> CalculateDistance(long driverId, Instant from, Instant to)
  {
    var left = TimeSlot.ThatContains(from);
    var right = TimeSlot.ThatContains(to);
    return Distance.OfKm(await _travelledDistanceRepository.CalculateDistance(left.Beginning, right.End, driverId));
  }

  public async Task AddPosition(DriverPosition driverPosition)
  {
    var driverId = driverPosition.Driver.Id.Value;
    var matchedSlot = await _travelledDistanceRepository
      .FindTravelledDistanceTimeSlotByTime(driverPosition.SeenAt, driverId);
    var now = _clock.GetCurrentInstant();
    if (matchedSlot != null)
    {
      if (matchedSlot.Contains(now))
      {
        AddDistanceToSlot(driverPosition, matchedSlot);
      }
      else if (matchedSlot.IsBefore(now))
      {
        RecalculateDistanceFor(matchedSlot, driverId);
      }
    }
    else
    {
      var currentTimeSlot = TimeSlot.ThatContains(now);
      var prev = currentTimeSlot.Prev();
      var prevTravelledDistance = await _travelledDistanceRepository.FindTravelledDistanceByTimeSlotAndDriverId(prev, driverId);
      if (prevTravelledDistance != null)
      {
        if (prevTravelledDistance.EndsAt(driverPosition.SeenAt))
        {
          AddDistanceToSlot(driverPosition, prevTravelledDistance);
        }
      }

      await CreateSlotForNow(driverPosition, driverId, currentTimeSlot);
    }
  }

  private void AddDistanceToSlot(DriverPosition driverPosition, TravelledDistance aggregatedDistance)
  {
    var travelled = Distance.OfKm(_distanceCalculator.CalculateByGeo(
      driverPosition.Latitude,
      driverPosition.Longitude,
      aggregatedDistance.LastLatitude,
      aggregatedDistance.LastLongitude));
    aggregatedDistance.AddDistance(travelled, driverPosition.Latitude, driverPosition.Longitude);
  }

  private void RecalculateDistanceFor(TravelledDistance aggregatedDistance, long driverId)
  {
    //TODO
  }

  private async Task CreateSlotForNow(DriverPosition driverPosition, long driverId, TimeSlot timeSlot)
  {
    await _travelledDistanceRepository.Save(new TravelledDistance(driverId, timeSlot, driverPosition));
  }
}