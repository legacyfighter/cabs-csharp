using LegacyFighter.Cabs.Assignment;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Pricing;
using NodaTime;

namespace LegacyFighter.Cabs.Ride.Details;

public interface ITransitDetailsFacade
{
  Task<TransitDetailsDto> Find(long? transitId);
  Task TransitRequested(Instant when, Guid requestId, Address from, Address to, Distance distance,
    Client client, CarClasses? carClass, Money estimatedPrice, Tariff tariff);
  Task PickupChangedTo(Guid requestId, Address newAddress, Distance newDistance);
  Task DestinationChanged(Guid requestId, Address newAddress, Distance newDistance);
  Task TransitPublished(Guid requestId, Instant when);
  Task TransitStarted(Guid requestId, long? transitId, Instant when);
  Task TransitAccepted(Guid requestId, long? driverId, Instant when);
  Task TransitCancelled(Guid requestId);
  Task TransitCompleted(Guid requestId, Instant when, Money price, Money driverFee);
  Task<List<TransitDetailsDto>> FindByClient(long? clientId);
  Task<List<TransitDetailsDto>> FindByDriver(long? driverId, Instant from, Instant to);
  Task<List<TransitDetailsDto>> FindCompleted();
  Task DriversAreInvolved(Guid requestId, InvolvedDriversSummary involvedDriversSummary);
  Task<TransitDetailsDto> Find(Guid requestId);
}