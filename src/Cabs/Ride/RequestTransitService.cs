using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.Pricing;
using NodaTime;

namespace LegacyFighter.Cabs.Ride;

public class RequestTransitService : IRequestTransitService
{
  private readonly Tariffs _tariffs;
  private readonly IGeocodingService _geocodingService;
  private readonly DistanceCalculator _distanceCalculator;
  private readonly IClock _clock;
  private readonly IRequestForTransitRepository _requestForTransitRepository;

  public RequestTransitService(
    Tariffs tariffs,
    IGeocodingService geocodingService,
    DistanceCalculator distanceCalculator,
    IClock clock,
    IRequestForTransitRepository requestForTransitRepository)
  {
    _tariffs = tariffs;
    _geocodingService = geocodingService;
    _distanceCalculator = distanceCalculator;
    _clock = clock;
    _requestForTransitRepository = requestForTransitRepository;
  }

  public async Task<RequestForTransit> CreateRequestForTransit(Address from, Address to)
  {
    // FIXME later: add some exceptions handling
    var geoFrom = _geocodingService.GeocodeAddress(from);
    var geoTo = _geocodingService.GeocodeAddress(to);
    var distance =
      Distance.OfKm((float)_distanceCalculator.CalculateByMap(geoFrom[0], geoFrom[1], geoTo[0], geoTo[1]));
    var now = _clock.GetCurrentInstant();
    var tariff = ChooseTariff(now);
    return await _requestForTransitRepository.Save(new RequestForTransit(tariff, distance));
  }

  private Tariff ChooseTariff(Instant when)
  {
    return _tariffs.Choose(when);
  }

  public async Task<Guid> FindCalculationGuid(long? requestId)
  {
    return (await _requestForTransitRepository.Find(requestId)).RequestGuid;
  }

  public async Task<Tariff> FindTariff(Guid requestGuid)
  {
    return (await _requestForTransitRepository.FindByRequestGuid(requestGuid)).Tariff;
  }
}