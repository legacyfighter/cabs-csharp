using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;

namespace LegacyFighter.Cabs.Ride;

public interface ICompleteTransitService
{
  Task<Money> CompleteTransit(long? driverId, Guid requestGuid, Address from, Address to);
}