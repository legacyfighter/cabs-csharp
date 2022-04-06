using LegacyFighter.Cabs.Assignment;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.Cabs.Ride;

public class ChangePickupService : IChangePickupService
{
  private readonly IAddressRepository _addressRepository;
  private readonly ITransitDemandRepository _transitDemandRepository;
  private readonly IDriverAssignmentFacade _driverAssignmentFacade;
  private IGeocodingService _geocodingService;
  private readonly DistanceCalculator _distanceCalculator;

  public ChangePickupService(
    IAddressRepository addressRepository,
    ITransitDemandRepository transitDemandRepository,
    IDriverAssignmentFacade driverAssignmentFacade, 
    IGeocodingService geocodingService, 
    DistanceCalculator distanceCalculator)
  {
    _addressRepository = addressRepository;
    _transitDemandRepository = transitDemandRepository;
    _driverAssignmentFacade = driverAssignmentFacade;
    _geocodingService = geocodingService;
    _distanceCalculator = distanceCalculator;
  }

  public async Task<Distance> ChangeTransitAddressFrom(
    Guid requestGuid, 
    Address newAddress,
    Address oldAddress)
  {
    newAddress = await _addressRepository.Save(newAddress);
    var transitDemand = await _transitDemandRepository.FindByTransitRequestGuid(requestGuid);
    if (transitDemand == null)
    {
      throw new InvalidOperationException($"Transit does not exist, id = {requestGuid}");
    }

    if (await _driverAssignmentFacade.IsDriverAssigned(requestGuid))
    {
      throw new InvalidOperationException($"Driver already assigned, requestGuid = {requestGuid}");
    }

    // TODO FIXME later: add some exceptions handling
    var geoFromNew = _geocodingService.GeocodeAddress(newAddress);
    var geoFromOld = _geocodingService.GeocodeAddress(oldAddress);

    // https://www.geeksforgeeks.org/program-distance-two-points-earth/
    // Using extension method ToRadians which converts from
    // degrees to radians.
    var lon1 = geoFromNew[1].ToRadians();
    var lon2 = geoFromOld[1].ToRadians();
    var lat1 = geoFromNew[0].ToRadians();
    var lat2 = geoFromOld[0].ToRadians();

    // Haversine formula
    var dlon = lon2 - lon1;
    var dlat = lat2 - lat1;
    var a = Math.Pow(Math.Sin(dlat / 2), 2)
            + Math.Cos(lat1) * Math.Cos(lat2)
                             * Math.Pow(Math.Sin(dlon / 2), 2);

    var c = 2 * Math.Asin(Math.Sqrt(a));

    // Radius of earth in kilometers. Use 3956 for miles
    double r = 6371;

    // calculate the result
    var distanceInKMeters = c * r;

    var newDistance = Distance.OfKm(
      (float) _distanceCalculator.CalculateByMap(geoFromNew[0], geoFromNew[1], geoFromOld[0], geoFromOld[1]));
    transitDemand.ChangePickup(distanceInKMeters);
    return newDistance;
  }

  public async Task ChangeTransitAddressFrom(
    Guid requestGuid, 
    AddressDto newAddress,
    AddressDto oldAddress) 
  {
    await ChangeTransitAddressFrom(
      requestGuid,
      newAddress.ToAddressEntity(),
      oldAddress.ToAddressEntity());
  }
}