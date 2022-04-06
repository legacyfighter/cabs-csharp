using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using NodaTime;

namespace LegacyFighter.CabsTests.Common;

public class RideFixture
{
  private readonly ITransitRepository _transitRepository;
  private readonly IAddressRepository _addressRepository;
  private readonly ITransitService _transitService;
  private readonly CarTypeFixture _carTypeFixture;
  private readonly StubbedTransitPrice _stubbedPrice;

  public RideFixture(
    ITransitRepository transitRepository,
    IAddressRepository addressRepository,
    ITransitService transitService,
    CarTypeFixture carTypeFixture, 
    StubbedTransitPrice stubbedPrice)
  {
    _transitRepository = transitRepository;
    _addressRepository = addressRepository;
    _carTypeFixture = carTypeFixture;
    _stubbedPrice = stubbedPrice;
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
    await _carTypeFixture.AnActiveCarCategory(CarType.CarClasses.Van);
    var transit = await _transitService.CreateTransit(
      client.Id, from, destination, CarType.CarClasses.Van);
    await _transitService.PublishTransit(transit.Id);
    await _transitService.FindDriversForTransit(transit.Id);
    await _transitService.AcceptTransit(driver.Id, transit.Id);
    await _transitService.StartTransit(driver.Id, transit.Id);
    await _transitService.CompleteTransit(driver.Id, transit.Id, destination);
    await _stubbedPrice.Stub(transit.Id, new Money(price));
    return await _transitRepository.Find(transit.Id);
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
    await _carTypeFixture.AnActiveCarCategory(CarType.CarClasses.Van);
    var transit = await _transitService.CreateTransit(
      client.Id, from, destination, CarType.CarClasses.Van);
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