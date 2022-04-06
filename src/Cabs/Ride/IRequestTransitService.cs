using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.Pricing;

namespace LegacyFighter.Cabs.Ride;

public interface IRequestTransitService
{
  Task<RequestForTransit> CreateRequestForTransit(Address from, Address to);
  Task<Tariff> FindTariff(Guid requestGuid);
  Task<Guid> FindCalculationGuid(long? requestId);
}