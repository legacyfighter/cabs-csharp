using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.Cabs.Ride;

public class ChangeDestinationService : IChangeDestinationService
{
  private readonly IGeocodingService _geocodingService;
  private readonly DistanceCalculator _distanceCalculator;
  private readonly ITransitRepository _transitRepository;

  public ChangeDestinationService(
    IGeocodingService geocodingService, 
    DistanceCalculator distanceCalculator, 
    ITransitRepository transitRepository)
  {
    _geocodingService = geocodingService;
    _distanceCalculator = distanceCalculator;
    _transitRepository = transitRepository;
  }

  public async Task<Distance> ChangeTransitAddressTo(Guid requestGuid, Address newAddress, Address from)
  {
    // TODO FIXME later: add some exceptions handling
    var geoFrom = _geocodingService.GeocodeAddress(from);
    var geoTo = _geocodingService.GeocodeAddress(newAddress);

    var newDistance = Distance.OfKm((float) _distanceCalculator.CalculateByMap(geoFrom[0], geoFrom[1], geoTo[0], geoTo[1]));
    var transit = await _transitRepository.FindByTransitRequestGuid(requestGuid);
    if (transit != null)
    {
      transit.ChangeDestination(newDistance);
    }

    return newDistance;
  }
}