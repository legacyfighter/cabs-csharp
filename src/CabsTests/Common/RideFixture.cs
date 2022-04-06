using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Ride;
using LegacyFighter.Cabs.Ride.Details;
using LegacyFighter.Cabs.Tracking;
using NodaTime;

namespace LegacyFighter.CabsTests.Common;

public class RideFixture
{
  private readonly ITransitRepository _transitRepository;
  private readonly IAddressRepository _addressRepository;
  private readonly ITransitService _transitService;
  private readonly IDriverSessionService _driverSessionService;
  private readonly CarTypeFixture _carTypeFixture;
  private readonly DriverFixture _driverFixture;
  private readonly StubbedTransitPrice _stubbedPrice;
  private readonly ITransitDetailsFacade _transitDetailsFacade;

  public RideFixture(
    ITransitRepository transitRepository,
    IAddressRepository addressRepository,
    ITransitService transitService,
    IDriverSessionService driverSessionService,
    CarTypeFixture carTypeFixture, 
    DriverFixture driverFixture,
    StubbedTransitPrice stubbedPrice, 
    ITransitDetailsFacade transitDetailsFacade)
  {
    _transitRepository = transitRepository;
    _addressRepository = addressRepository;
    _carTypeFixture = carTypeFixture;
    _driverFixture = driverFixture;
    _stubbedPrice = stubbedPrice;
    _transitDetailsFacade = transitDetailsFacade;
    _transitService = transitService;
    _driverSessionService = driverSessionService;
  }

  public async Task<Transit> ARide(
    int price,
    Client client,
    Driver driver,
    Address from,
    Address destination)
  {
    StubPrice(price);
    from = await _addressRepository.Save(from);
    destination = await _addressRepository.Save(destination);
    await _carTypeFixture.AnActiveCarCategory(CarClasses.Van);
    var transitView = await _transitService.CreateTransit(client.Id, from, destination, CarClasses.Van);
    await _transitService.PublishTransit(transitView.RequestId);
    await _transitService.FindDriversForTransit(transitView.RequestId);
    await _transitService.AcceptTransit(driver.Id, transitView.RequestId);
    await _transitService.StartTransit(driver.Id, transitView.RequestId);
    await _transitService.CompleteTransit(driver.Id, transitView.RequestId, destination);
    var transitId = (await _transitDetailsFacade.Find(transitView.RequestId)).TransitId;
    return await _transitRepository.Find(transitId);

  }

  private void StubPrice(int price) 
  {
    var fakePrice = new Money(price);
    _stubbedPrice.Stub(fakePrice);
  }

  public async Task<TransitDto> ARideWithFixedClock(
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
    _stubbedPrice.Stub(new Money(price));

    await _carTypeFixture.AnActiveCarCategory(CarClasses.Van);
    var transit = await _transitService.CreateTransit(client.Id, from, destination, CarClasses.Van);
    await _transitService.PublishTransit(transit.RequestId);
    await _transitService.FindDriversForTransit(transit.RequestId);
    await _transitService.AcceptTransit(driver.Id, transit.RequestId);
    await _transitService.StartTransit(driver.Id, transit.RequestId);
    clock.GetCurrentInstant().Returns(completedAt);
    await _transitService.CompleteTransit(driver.Id, transit.RequestId, destination);
    return await _transitService.LoadTransit(transit.RequestId);
  }

  public async Task<TransitDto> DriverHasDoneSessionAndPicksSomeoneUpInCar(
    Driver driver,
    Client client,
    CarClasses carClass,
    string plateNumber,
    string carBrand,
    Instant when,
    IGeocodingService geocodingService,
    IClock clock)
  {
    clock.GetCurrentInstant().Returns(when);
    var from = await _addressRepository.Save(new Address("PL", "MAZ", "WAW", "STREET", 1));
    var to = await _addressRepository.Save(new Address("PL", "MAZ", "WAW", "STREET", 100));
    geocodingService.GeocodeAddress(Arg.Any<Address>()).Returns(new double[] { 1, 1 });
    await _driverFixture.DriverLogsIn(plateNumber, carClass, driver, carBrand);
    await _driverFixture.DriverIsAtGeoLocalization(plateNumber, 1, 1, carClass, driver, when, carBrand);
    await _driverFixture.DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);
    var transit = await ARideWithFixedClock(30, when, when, client, driver, from, to, clock);
    await _driverSessionService.LogOutCurrentSession(driver.Id);
    return transit;
  }

}