using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using LegacyFighter.Cabs.TransitDetail;
using NodaTime;

namespace LegacyFighter.CabsTests.Common;

public class RideFixture
{
  private readonly ITransitRepository _transitRepository;
  private readonly IAddressRepository _addressRepository;
  private readonly ITransitService _transitService;
  private readonly CarTypeFixture _carTypeFixture;
  private readonly StubbedTransitPrice _stubbedPrice;
  private readonly ITransitDetailsFacade _transitDetailsFacade;

  public RideFixture(
    ITransitRepository transitRepository,
    IAddressRepository addressRepository,
    ITransitService transitService,
    CarTypeFixture carTypeFixture, 
    StubbedTransitPrice stubbedPrice, 
    ITransitDetailsFacade transitDetailsFacade)
  {
    _transitRepository = transitRepository;
    _addressRepository = addressRepository;
    _carTypeFixture = carTypeFixture;
    _stubbedPrice = stubbedPrice;
    _transitDetailsFacade = transitDetailsFacade;
    _transitService = transitService;
  }

  public async Task<Transit> ARide(
    int price,
    Client client,
    Driver driver,
    Address from,
    Address destination) 
  {
    from = await _addressRepository.Save(from);
    destination = await _addressRepository.Save(destination);
    await _carTypeFixture.AnActiveCarCategory(CarClasses.Van);
    var transit = await _transitService.CreateTransit(
      client.Id, from, destination, CarClasses.Van);
    await _transitService.PublishTransit(transit.Id);
    await _transitService.FindDriversForTransit(transit.Id);
    await _transitService.AcceptTransit(driver.Id, transit.Id);
    await _transitService.StartTransit(driver.Id, transit.Id);
    await _transitService.CompleteTransit(driver.Id, transit.Id, destination);
    await StubPrice(price, transit);
    return await _transitRepository.Find(transit.Id);
  }

  private async Task StubPrice(int price, Transit transit) 
  {
    var fakePrice = new Money(price);
    await _stubbedPrice.Stub(transit.Id, fakePrice);
    await _transitDetailsFacade.TransitCompleted(
      transit.Id, SystemClock.Instance.GetCurrentInstant(), fakePrice, fakePrice);
  }

  public async Task<Transit> ARideWithFixedClock(
    int price,
    Instant publishedAt,
    Instant completedAt,
    Client client,
    Driver driver,
    Address from,
    Address destination,
    IClock clock) 
  {
    from = await _addressRepository.Save(from);
    destination = await _addressRepository.Save(destination);
    clock.GetCurrentInstant().Returns(publishedAt);
    await _carTypeFixture.AnActiveCarCategory(CarClasses.Van);
    var transit = await _transitService.CreateTransit(
      client.Id, from, destination, CarClasses.Van);
    await _transitService.PublishTransit(transit.Id);
    await _transitService.FindDriversForTransit(transit.Id);
    await _transitService.AcceptTransit(driver.Id, transit.Id);
    await _transitService.StartTransit(driver.Id, transit.Id);
    clock.GetCurrentInstant().Returns(completedAt);
    await _transitService.CompleteTransit(driver.Id, transit.Id, destination);
    await _stubbedPrice.Stub(transit.Id, new Money(price));
    return await _transitRepository.Find(transit.Id);
  }
}