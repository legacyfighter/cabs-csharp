using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;
using NodaTime;

namespace LegacyFighter.Cabs.TransitDetail;

public interface ITransitDetailsFacade
{
  Task<TransitDetailsDto> Find(long? transitId);
  Task TransitRequested(Instant when, long? transitId, Address from, Address to, Distance distance,
    Client client, CarClasses? carClass, Money estimatedPrice, Tariff tariff);
  Task PickupChangedTo(long? transitId, Address newAddress, Distance newDistance);
  Task DestinationChanged(long? transitId, Address newAddress, Distance newDistance);
  Task TransitPublished(long? transitId, Instant when);
  Task TransitStarted(long? transitId, Instant when);
  Task TransitAccepted(long? transitId, Instant when, long? driverId);
  Task TransitCancelled(long? transitId);
  Task TransitCompleted(long? transitId, Instant when, Money price, Money driverFee);
  Task<List<TransitDetailsDto>> FindByClient(long? clientId);
  Task<List<TransitDetailsDto>> FindByDriver(long? driverId, Instant from, Instant to);
  Task<List<TransitDetailsDto>> FindCompleted();
}