using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;

namespace LegacyFighter.Cabs.Ride;

public class CompleteTransitService : ICompleteTransitService
{
  private readonly ITransitRepository _transitRepository;
  private readonly IGeocodingService _geocodingService;
  private readonly DistanceCalculator _distanceCalculator;

  public CompleteTransitService(
    ITransitRepository transitRepository,
    IGeocodingService geocodingService,
    DistanceCalculator distanceCalculator)
  {
    _transitRepository = transitRepository;
    _geocodingService = geocodingService;
    _distanceCalculator = distanceCalculator;
  }

  public async Task<Money> CompleteTransit(long? driverId, Guid requestGuid, Address from, Address destinationAddress)
  {
    var transit = await _transitRepository.FindByTransitRequestGuid(requestGuid);

    if (transit == null)
    {
      throw new ArgumentException($"Transit does not exist, id = {requestGuid}");
    }

    // FIXME later: add some exceptions handling
    var geoFrom = _geocodingService.GeocodeAddress(from);
    var geoTo = _geocodingService.GeocodeAddress(destinationAddress);
    var distance = Distance.OfKm(
      (float)_distanceCalculator.CalculateByMap(geoFrom[0], geoFrom[1], geoTo[0], geoTo[1]));
    var finalPrice = transit.CompleteTransitAt(distance);
    await _transitRepository.Save(transit);
    return finalPrice;
  }
}