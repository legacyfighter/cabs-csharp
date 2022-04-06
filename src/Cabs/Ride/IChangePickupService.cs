using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.Cabs.Ride;

public interface IChangePickupService
{
  Task<Distance> ChangeTransitAddressFrom(
    Guid requestGuid, 
    Address newAddress,
    Address oldAddress);

  Task ChangeTransitAddressFrom(
    Guid requestGuid, 
    AddressDto newAddress,
    AddressDto oldAddress);
}