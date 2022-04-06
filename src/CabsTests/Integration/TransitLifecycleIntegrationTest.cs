using System;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.Ride;
using LegacyFighter.Cabs.Ride.Details;
using LegacyFighter.CabsTests.Common;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

public class TransitLifeCycleIntegrationTest
{
  private Fixtures Fixtures => _app.Fixtures;
  private IRideService RideService => _app.RideService;
  private IGeocodingService GeocodingService { get; set; } = default!;
  private CabsApp _app = default!;

  [SetUp]
  public async Task Setup()
  {
    GeocodingService = Substitute.For<IGeocodingService>();
    _app = CabsApp.CreateInstance(collection => { collection.AddSingleton(GeocodingService); });
    await Fixtures.AnActiveCarCategory(CarClasses.Van);
  }

  [Test]
  public async Task CanCreateTransit()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    await ANearbyDriver(pickup);

    //when
    var transit = await RequestTransitFromTo(pickup, destination);

    //then
    var loaded = await RideService.LoadTransit(transit.RequestId);
    Assert.IsNull(loaded.CarClass);
    Assert.IsNull(loaded.ClaimDto);
    Assert.NotNull(loaded.EstimatedPrice);
    Assert.IsNull(loaded.Price);
    Assert.AreEqual("Polska", loaded.From.Country);
    Assert.AreEqual("Warszawa", loaded.From.City);
    Assert.AreEqual("Młynarska", loaded.From.Street);
    Assert.AreEqual(20, loaded.From.BuildingNumber);
    Assert.AreEqual("Polska", loaded.To.Country);
    Assert.AreEqual("Warszawa", loaded.To.City);
    Assert.AreEqual("Żytnia", loaded.To.Street);
    Assert.AreEqual(25, loaded.To.BuildingNumber);
    Assert.AreEqual(Statuses.Draft, loaded.Status);
    Assert.NotNull(loaded.Tariff);
    Assert.AreNotEqual(0, loaded.KmRate);
    Assert.NotNull(loaded.DateTime);
  }

  [Test]
  public async Task CanChangeTransitDestination()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);

    //when
    var newDestination = await NewAddress("Polska", "Warszawa", "Mazowiecka", 30);
    //and
    await RideService.ChangeTransitAddressTo(transit.RequestId, newDestination);

    //then
    var loaded = await RideService.LoadTransit(transit.RequestId);
    Assert.AreEqual(30, loaded.To.BuildingNumber);
    Assert.AreEqual("Mazowiecka", loaded.To.Street);
    Assert.NotNull(loaded.EstimatedPrice);
    Assert.IsNull(loaded.Price);
  }

  [Test]
  public async Task CannotChangeDestinationWhenTransitIsCompleted()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var driver = await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    await RideService.PublishTransit(transit.RequestId);
    //and
    await RideService.AcceptTransit(driver, transit.RequestId);
    //and
    await RideService.StartTransit(driver, transit.RequestId);
    //and
    await RideService.CompleteTransit(driver, transit.RequestId, destination);

    //expect
    var newAddress = await NewAddress("Polska", "Warszawa", "Żytnia", 23);
    await RideService.Awaiting(s => s.ChangeTransitAddressTo(transit.RequestId, newAddress))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CanChangePickupPlace()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    await RideService.PublishTransit(transit.RequestId);

    //when
    var newPickup = NewPickupAddress("Puławska", 28);
    //and
    await RideService.ChangeTransitAddressFrom(transit.RequestId, newPickup);

    //then
    var loaded = await RideService.LoadTransit(transit.RequestId);
    Assert.AreEqual(28, loaded.From.BuildingNumber);
    Assert.AreEqual("Puławska", loaded.From.Street);
  }

  [Test]
  public async Task CannotChangePickupPlaceAfterTransitIsAccepted()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var driver = await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    var changedTo = NewPickupAddress(10);
    //and
    await RideService.PublishTransit(transit.RequestId);
    //and
    await RideService.AcceptTransit(driver, transit.RequestId);

    //expect
    await RideService.Awaiting(s => s.ChangeTransitAddressFrom(transit.RequestId, changedTo))
      .Should().ThrowExactlyAsync<InvalidOperationException>();

    //and
    await RideService.StartTransit(driver, transit.RequestId);
    //expect
    await RideService.Awaiting(s => s.ChangeTransitAddressFrom(transit.RequestId, changedTo))
      .Should().ThrowExactlyAsync<InvalidOperationException>();

    //and
    await RideService.CompleteTransit(driver, transit.RequestId, destination);
    //expect
    await RideService.Awaiting(s => s.ChangeTransitAddressFrom(transit.RequestId, changedTo))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CannotChangePickupPlaceMoreThanThreeTimes()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    await RideService.PublishTransit(transit.RequestId);
    //and
    var newPickup1 = NewPickupAddress(10);
    await RideService.ChangeTransitAddressFrom(transit.RequestId, newPickup1);
    //and
    var newPickup2 = NewPickupAddress(11);
    await RideService.ChangeTransitAddressFrom(transit.RequestId, newPickup2);
    //and
    var newPickup3 = NewPickupAddress(12);
    await RideService.ChangeTransitAddressFrom(transit.RequestId, newPickup3);

    //expect
    await RideService.Awaiting(
        s => s.ChangeTransitAddressFrom(transit.RequestId, NewPickupAddress(13)))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CannotChangePickupPlaceWhenItIsFarWayFromOriginal()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    await RideService.PublishTransit(transit.RequestId);

    //expect
    var farawayAddress = await FarAwayAddress();
    await RideService.Awaiting(
        s => s.ChangeTransitAddressFrom(transit.RequestId, farawayAddress))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CanCancelTransit()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);

    //when
    await RideService.CancelTransit(transit.RequestId);

    //then
    var loaded = await RideService.LoadTransit(transit.RequestId);
    Assert.AreEqual(Statuses.Cancelled, loaded.Status);
  }

  [Test]
  public async Task CannotCancelTransitAfterItWasStarted()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var driver = await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    await RideService.PublishTransit(transit.RequestId);
    //and
    await RideService.AcceptTransit(driver, transit.RequestId);

    //and
    await RideService.StartTransit(driver, transit.RequestId);
    //expect
    await RideService.Awaiting(s => s.CancelTransit(transit.RequestId))
      .Should().ThrowExactlyAsync<InvalidOperationException>();

    //and
    await RideService.CompleteTransit(driver, transit.RequestId, destination);
    //expect
    await RideService.Awaiting(s => s.CancelTransit(transit.RequestId))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CanPublishTransit()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);

    //when
    await RideService.PublishTransit(transit.RequestId);

    //then
    var loaded = await RideService.LoadTransit(transit.RequestId);
    Assert.AreEqual(Statuses.WaitingForDriverAssignment, loaded.Status);
    Assert.NotNull(loaded.Published);
  }

  [Test]
  public async Task CanAcceptTransit()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var driver = await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    await RideService.PublishTransit(transit.RequestId);

    //when
    await RideService.AcceptTransit(driver, transit.RequestId);

    //then
    var loaded = await RideService.LoadTransit(transit.RequestId);
    Assert.AreEqual(Statuses.TransitToPassenger, loaded.Status);
    Assert.NotNull(loaded.AcceptedAt);
  }

  [Test]
  public async Task OnlyOneDriverCanAcceptTransit()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var driver = await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    var secondDriver = await ANearbyDriver(pickup);
    //and
    await RideService.PublishTransit(transit.RequestId);
    //and
    await RideService.AcceptTransit(driver, transit.RequestId);

    //expect
    await RideService.Awaiting(s => s.AcceptTransit(secondDriver, transit.RequestId))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task TransitCannotBeAcceptedByDriverWhoAlreadyRejected()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var driver = await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    await RideService.PublishTransit(transit.RequestId);

    //and
    await RideService.RejectTransit(driver, transit.RequestId);

    //expect
    await RideService.Awaiting(s => s.AcceptTransit(driver, transit.RequestId))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task TransitCannotBeAcceptedByDriverWhoHasNotSeenProposal()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var farAwayDriver = await AFarAwayDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    await RideService.PublishTransit(transit.RequestId);

    //expect
    await RideService.Awaiting(s => s.AcceptTransit(farAwayDriver, transit.RequestId))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CanStartTransit()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var driver = await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    await RideService.PublishTransit(transit.RequestId);
    //and
    await RideService.AcceptTransit(driver, transit.RequestId);
    //when
    await RideService.StartTransit(driver, transit.RequestId);

    //then
    var loaded = await RideService.LoadTransit(transit.RequestId);
    Assert.AreEqual(Statuses.InTransit, loaded.Status);
    Assert.NotNull(loaded.Started);
  }

  [Test]
  public async Task CannotStartNotAcceptedTransit()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var driver = await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    await RideService.PublishTransit(transit.RequestId);

    //expect
    await RideService.Awaiting(s => s.StartTransit(driver, transit.RequestId))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CanCompleteTransit()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var driver = await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    await RideService.PublishTransit(transit.RequestId);
    //and
    await RideService.AcceptTransit(driver, transit.RequestId);
    //and
    await RideService.StartTransit(driver, transit.RequestId);

    //when
    await RideService.CompleteTransit(driver, transit.RequestId, destination);

    //then
    var loaded = await RideService.LoadTransit(transit.RequestId);
    Assert.AreEqual(Statuses.Completed, loaded.Status);
    Assert.NotNull(loaded.Tariff);
    Assert.NotNull(loaded.Price);
    Assert.NotNull(loaded.DriverFee);
    Assert.NotNull(loaded.CompleteAt);
  }

  [Test]
  public async Task CannotCompleteNotStartedTransit()
  {
    //given
    var addressTo = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var pickup = new AddressDto(null, null, null, 0);
    //and
    var driver = await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, addressTo);
    //and
    await RideService.PublishTransit(transit.RequestId);
    //and
    await RideService.AcceptTransit(driver, transit.RequestId);

    //expect
    await RideService.Awaiting(s => s.CompleteTransit(driver, transit.RequestId, addressTo))
      .Should().ThrowExactlyAsync<ArgumentException>();
  }

  [Test]
  public async Task CanRejectTransit()
  {
    //given
    var pickup = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    //and
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var driver = await ANearbyDriver(pickup);
    //and
    var transit = await RequestTransitFromTo(pickup, destination);
    //and
    await RideService.PublishTransit(transit.RequestId);

    //when
    await RideService.RejectTransit(driver, transit.RequestId);

    //then
    var loaded = await RideService.LoadTransit(transit.RequestId);
    Assert.AreEqual(Statuses.WaitingForDriverAssignment, loaded.Status);
    Assert.IsNull(loaded.AcceptedAt);
  }

  private async Task<AddressDto> NewAddress(
    string country,
    string city,
    string street,
    int buildingNumber)
  {
    var addressDto = await Fixtures.AnAddress(
      GeocodingService,
      country,
      city,
      street,
      buildingNumber);
    GeocodingService.GeocodeAddress(
        Arg.Is<Address>(a => new AddressMatcher(addressDto).Matches(a)))
      .Returns(new double[] { 1, 1 });
    return addressDto;
  }

  private async Task<AddressDto> FarAwayAddress()
  {
    var addressDto = await NewAddress("Dania", "Kopenhaga", "Mylve", 2);
    GeocodingService.GeocodeAddress(
        Arg.Is<Address>(a => new AddressMatcher(addressDto).Matches(a)))
      .Returns(new double[] { 10000, 21211321 });
    return addressDto;
  }

  private async Task<long?> ANearbyDriver(AddressDto from)
  {
    return (await Fixtures.ANearbyDriver(GeocodingService, from.ToAddressEntity(), 1, 1)).Id;
  }

  private async Task<long?> AFarAwayDriver(AddressDto address)
  {
    GeocodingService.GeocodeAddress(
        Arg.Is<Address>(a => new AddressMatcher(address).Matches(a)))
      .Returns(new double[] { 20000000, 100000000 });
    return (await Fixtures.ANearbyDriver("DW MARIO", 1000000000, 1000000000, CarClasses.Van, SystemClock.Instance.GetCurrentInstant(), "BRAND")).Id;

  }

  private async Task<TransitDto> RequestTransitFromTo(AddressDto pickupDto, AddressDto destination)
  {
    GeocodingService.GeocodeAddress(
        Arg.Is<Address>(a => new AddressMatcher(destination).Matches(a)))
      .Returns(new double[] { 1, 1 });
    var transitDto = await Fixtures.ATransitDto(pickupDto, destination);
    return await RideService.CreateTransit(transitDto);
  }

  private AddressDto NewPickupAddress(int buildingNumber) 
  {
    var newPickup = new AddressDto("Polska", "Warszawa", "Mazowiecka", buildingNumber);
    GeocodingService.GeocodeAddress(
        Arg.Is<Address>(a => new AddressMatcher(newPickup).Matches(a)))
      .Returns(new double[] { 1, 1 });
    return newPickup;
  }

  private AddressDto NewPickupAddress(string street, int buildingNumber) 
  {
    var newPickup = new AddressDto("Polska", "Warszawa", street, buildingNumber);
    GeocodingService.GeocodeAddress(
        Arg.Is<Address>(a => new AddressMatcher(newPickup).Matches(a)))
      .Returns(new double[] { 1, 1 });
    return newPickup;
  }
}