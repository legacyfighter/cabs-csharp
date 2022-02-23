using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Service;
using NodaTime;

namespace LegacyFighter.Cabs.DriverReports.TravelledDistances;

public interface ITravelledDistanceService
{
  Task<Distance> CalculateDistance(long driverId, Instant from, Instant to);
  Task AddPosition(long driverId, double latitude, double longitude, Instant seenAt);
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

  public async Task AddPosition(long driverId, double latitude, double longitude, Instant seenAt)
  {
    var matchedSlot = await _travelledDistanceRepository
      .FindTravelledDistanceTimeSlotByTime(seenAt, driverId);
    var now = _clock.GetCurrentInstant();
    if (matchedSlot != null)
    {
      if (matchedSlot.Contains(now))
      {
        AddDistanceToSlot(matchedSlot, latitude, longitude);
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
        if (prevTravelledDistance.EndsAt(seenAt))
        {
          AddDistanceToSlot(prevTravelledDistance, latitude, longitude);
        }
      }

      await CreateSlotForNow(driverId, currentTimeSlot, latitude, longitude);
    }
  }

  private void AddDistanceToSlot(TravelledDistance aggregatedDistance, double latitude, double longitude)
  {
    var travelled = Distance.OfKm(_distanceCalculator.CalculateByGeo(
      latitude,
      longitude,
      aggregatedDistance.LastLatitude,
      aggregatedDistance.LastLongitude));
    aggregatedDistance.AddDistance(travelled, latitude, longitude);
  }

  private void RecalculateDistanceFor(TravelledDistance aggregatedDistance, long driverId)
  {
    //TODO
  }

  private async Task CreateSlotForNow(long driverId, TimeSlot timeSlot, double latitude, double longitude)
  {
    await _travelledDistanceRepository.Save(new TravelledDistance(driverId, timeSlot, latitude, longitude));
  }
}