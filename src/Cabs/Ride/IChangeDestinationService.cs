using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.Cabs.Ride;

public interface IChangeDestinationService
{
  Task<Distance> ChangeTransitAddressTo(Guid requestGuid, Address newAddress, Address from);
}