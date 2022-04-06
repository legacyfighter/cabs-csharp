using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.TransitDetail;
using NodaTime;

namespace LegacyFighter.CabsTests.Common;

public class TransitFixture
{
  private readonly ITransitRepository _transitRepository;
  private readonly ITransitDetailsFacade _transitDetailsFacade;
  private readonly StubbedTransitPrice _stubbedTransitPrice;

  public TransitFixture(
    ITransitRepository transitRepository,
    ITransitDetailsFacade transitDetailsFacade,
    StubbedTransitPrice stubbedTransitPrice)
  {
    _transitRepository = transitRepository;
    _transitDetailsFacade = transitDetailsFacade;
    _stubbedTransitPrice = stubbedTransitPrice;
  }

  public async Task<Transit> TransitDetails(Driver driver, int price, LocalDateTime when, Client client, Address from,
    Address to)
  {
    var transit = await _transitRepository.Save(new Transit());
    await _stubbedTransitPrice.Stub(transit.Id, new Money(price));
    var transitId = transit.Id;
    await _transitDetailsFacade.TransitRequested(
      when.InUtc().ToInstant(),
      transitId,
      from,
      to,
      Distance.Zero,
      client,
      CarType.CarClasses.Van,
      new Money(price),
      Tariff.OfTime(when));
    await _transitDetailsFacade.TransitAccepted(transitId, when.InUtc().ToInstant(), driver.Id);
    await _transitDetailsFacade.TransitStarted(transitId, when.InUtc().ToInstant());
    await _transitDetailsFacade.TransitCompleted(transitId, when.InUtc().ToInstant(), new Money(price), null);
    return transit;
  }

  public TransitDto ATransitDto(Client client, AddressDto from, AddressDto to)
  {
    var transitDto = new TransitDto
    {
      ClientDto = new ClientDto(client),
      From = from,
      To = to
    };
    return transitDto;
  }
}