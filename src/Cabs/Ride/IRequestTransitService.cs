using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.Cabs.Ride;

public interface IRequestTransitService
{
  Task<RequestForTransit> CreateRequestForTransit(Address from, Address to);
}