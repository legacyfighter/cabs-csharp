using System;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Pricing;
using LegacyFighter.Cabs.Ride;
using LegacyFighter.Cabs.Ride.Details;
using LocalDateTime = NodaTime.LocalDateTime;

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

  public async Task<Transit> TransitDetails(
    Driver driver,
    int price,
    LocalDateTime when,
    Client client,
    Address from,
    Address to)
  {
    var transit = await _transitRepository.Save(new Transit(null, Guid.NewGuid()));
    _stubbedTransitPrice.Stub(new Money(price));
    await _transitDetailsFacade.TransitRequested(
      when.InUtc().ToInstant(),
      transit.RequestGuid,
      from,
      to,
      Distance.OfKm(20),
      client,
      CarClasses.Van,
      new Money(price),
      Tariff.OfTime(when));
    await _transitDetailsFacade.TransitAccepted(
      transit.RequestGuid,
      driver.Id,
      when.InUtc().ToInstant());
    await _transitDetailsFacade.TransitStarted(
      transit.RequestGuid,
      transit.Id,
      when.InUtc().ToInstant());
    await _transitDetailsFacade.TransitCompleted(
      transit.RequestGuid,
      when.InUtc().ToInstant(),
      new Money(price),
      null);
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