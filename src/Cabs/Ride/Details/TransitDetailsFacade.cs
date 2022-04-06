using LegacyFighter.Cabs.Assignment;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Pricing;
using NodaTime;

namespace LegacyFighter.Cabs.Ride.Details;

public class TransitDetailsFacade : ITransitDetailsFacade
{
  private readonly ITransitDetailsRepository _transitDetailsRepository;

  public TransitDetailsFacade(ITransitDetailsRepository transitDetailsRepository)
  {
    _transitDetailsRepository = transitDetailsRepository;
  }

  public async Task<TransitDetailsDto> Find(Guid requestId)
  {
    return new TransitDetailsDto(await Load(requestId));
  }

  public async Task<TransitDetailsDto> Find(long? transitId)
  {
    return new TransitDetailsDto(await Load(transitId));
  }

  public async Task TransitRequested(
    Instant when,
    Guid requestId,
    Address from,
    Address to,
    Distance distance,
    Client client,
    CarClasses? carClass,
    Money estimatedPrice,
    Tariff tariff)
  {
    var transitDetails =
      new TransitDetails(
        when,
        requestId,
        from,
        to,
        distance,
        client,
        carClass,
        estimatedPrice,
        tariff);
    await _transitDetailsRepository.Save(transitDetails);
  }

  public async Task PickupChangedTo(Guid requestId, Address newAddress, Distance newDistance)
  {
    var details = await Load(requestId);
    details.SetPickupChangedTo(newAddress, newDistance);
  }

  public async Task DestinationChanged(Guid requestId, Address newAddress, Distance newDistance)
  {
    var details = await Load(requestId);
    details.SetDestinationChangedTo(newAddress, newDistance);
  }

  public async Task TransitStarted(Guid requestId, long? transitId, Instant when)
  {
    var details = await Load(transitId);
    details.SetStartedAt(when, transitId);
  }

  public async Task TransitAccepted(Guid requestId, long? driverId, Instant when)
  {
    var details = await Load(requestId);
    details.SetAcceptedAt(when, driverId);
  }

  public async Task TransitCompleted(Guid requestId, Instant when, Money price, Money driverFee)
  {
    var details = await Load(requestId);
    details.SetCompletedAt(when, price, driverFee);
  }

  public async Task<List<TransitDetailsDto>> FindByClient(long? clientId) 
  {
    return (await _transitDetailsRepository.FindByClientId(clientId))
      .Select(td => new TransitDetailsDto(td))
      .ToList();
  }
  
  public async Task<List<TransitDetailsDto>> FindCompleted() 
  {
    return (await _transitDetailsRepository.FindByStatus(Statuses.Completed))
      .Select(t => new TransitDetailsDto(t))
      .ToList();
  }
  public async Task<List<TransitDetailsDto>> FindByDriver(long? driverId, Instant from, Instant to) 
  {
    return (await _transitDetailsRepository.FindAllByDriverAndDateTimeBetween(driverId, from, to))
      .Select(td => new TransitDetailsDto(td))
      .ToList();
  }

  private async Task<TransitDetails> Load(Guid requestId)
  {
    return await _transitDetailsRepository.FindByRequestGuid(requestId);
  }

  private async Task<TransitDetails> Load(long? transitId)
  {
    return await _transitDetailsRepository.FindByTransitId(transitId);
  }

  public async Task TransitPublished(Guid requestId, Instant when)
  {
    var details = await Load(requestId);
    details.SetPublishedAt(when);
  }

  public async Task DriversAreInvolved(Guid requestId, InvolvedDriversSummary involvedDriversSummary) 
  {
    var details = await Load(requestId);
    details.InvolvedDriversAre(involvedDriversSummary);
  }

  public async Task TransitCancelled(Guid requestId)
  {
    var details = await Load(requestId);
    details.SetAsCancelled();
  }
}