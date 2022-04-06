using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using NodaTime;

namespace LegacyFighter.Cabs.TransitDetail;

public class TransitDetailsFacade : ITransitDetailsFacade
{
  private readonly ITransitDetailsRepository _transitDetailsRepository;

  public TransitDetailsFacade(ITransitDetailsRepository transitDetailsRepository)
  {
    _transitDetailsRepository = transitDetailsRepository;
  }

  public async Task<TransitDetailsDto> Find(long? transitId)
  {
    return new TransitDetailsDto(await Load(transitId));
  }

  public async Task TransitRequested(Instant when, long? transitId, Address from, Address to, Distance distance,
    Client client, CarType.CarClasses? carClass, Money estimatedPrice, Tariff tariff)
  {
    var transitDetails =
      new TransitDetails(when, transitId, from, to, distance, client, carClass, estimatedPrice, tariff);
    await _transitDetailsRepository.Save(transitDetails);
  }

  public async Task PickupChangedTo(long? transitId, Address newAddress, Distance newDistance)
  {
    var details = await Load(transitId);
    details.SetPickupChangedTo(newAddress, newDistance);
  }

  public async Task DestinationChanged(long? transitId, Address newAddress, Distance newDistance)
  {
    var details = await Load(transitId);
    details.SetDestinationChangedTo(newAddress, newDistance);
  }

  public async Task TransitPublished(long? transitId, Instant when)
  {
    var details = await Load(transitId);
    details.SetPublishedAt(when);
  }

  public async Task TransitStarted(long? transitId, Instant when)
  {
    var details = await Load(transitId);
    details.SetStartedAt(when);
  }

  public async Task TransitAccepted(long? transitId, Instant when, long? driverId)
  {
    var details = await Load(transitId);
    details.SetAcceptedAt(when, driverId);
  }

  public async Task TransitCancelled(long? transitId)
  {
    var details = await Load(transitId);
    details.SetAsCancelled();
  }

  public async Task TransitCompleted(long? transitId, Instant when, Money price, Money driverFee)
  {
    var details = await Load(transitId);
    details.SetCompletedAt(when, price, driverFee);
  }

  public async Task<List<TransitDetailsDto>> FindByClient(long? clientId) 
  {
    return (await _transitDetailsRepository.FindByClientId(clientId))
      .Select(td => new TransitDetailsDto(td))
      .ToList();
  }

  public async Task<List<TransitDetailsDto>> FindByDriver(long? driverId, Instant from, Instant to) 
  {
    return (await _transitDetailsRepository.FindAllByDriverAndDateTimeBetween(driverId, from, to))
      .Select(td => new TransitDetailsDto(td))
      .ToList();
  }

  private Task<TransitDetails> Load(long? transitId)
  {
    return _transitDetailsRepository.FindByTransitId(transitId);
  }
}