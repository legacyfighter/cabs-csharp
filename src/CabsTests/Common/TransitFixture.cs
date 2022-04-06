using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.TransitDetail;
using NodaTime;
using NodaTime.Extensions;

namespace LegacyFighter.CabsTests.Common;

public class TransitFixture
{
  private readonly ITransitRepository _transitRepository;
  private readonly ITransitDetailsFacade _transitDetailsFacade;

  public TransitFixture(
    ITransitRepository transitRepository,
    ITransitDetailsFacade transitDetailsFacade)
  {
    _transitRepository = transitRepository;
    _transitDetailsFacade = transitDetailsFacade;
  }

  public async Task<Transit> ATransit(Driver driver, int price, LocalDateTime when, Client? client)
  {
    var dateTime = when.InUtc().ToInstant();
    var transit = new Transit(dateTime, Distance.Zero)
    {
      Price = new Money(price)
    };
    transit.ProposeTo(driver);
    transit.AcceptBy(driver, SystemClock.Instance.GetCurrentInstant());
    transit = await _transitRepository.Save(transit);
    await _transitDetailsFacade.TransitRequested(dateTime, transit.Id, null, null, Distance.Zero, client, null, new Money(price), transit.Tariff);
    return transit;
  }

  public async Task<Transit> ATransit(Driver driver, int price)
  {
    return await ATransit(driver, price, SystemClock.Instance.InBclSystemDefaultZone().GetCurrentLocalDateTime(), null);
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