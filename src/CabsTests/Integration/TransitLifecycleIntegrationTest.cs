using System;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Service;
using LegacyFighter.CabsTests.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LegacyFighter.CabsTests.Integration;

public class TransitLifeCycleIntegrationTest
{
  private Fixtures Fixtures => _app.Fixtures;
  private ITransitService TransitService => _app.TransitService;
  private IDriverSessionService DriverSessionService => _app.DriverSessionService;
  private IDriverTrackingService DriverTrackingService => _app.DriverTrackingService;
  private IGeocodingService GeocodingService { get; set; } = default!;
  private CabsApp _app = default!;

  [SetUp]
  public async Task Setup()
  {
    GeocodingService = Substitute.For<IGeocodingService>();
    _app = CabsApp.CreateInstance(collection => { collection.AddSingleton(GeocodingService); });
    await Fixtures.AnActiveCarCategory(CarType.CarClasses.Van);
    GeocodingService.GeocodeAddress(Arg.Any<Address>()).Returns(new double[] { 1, 1 });
  }

  [Test]
  public async Task CanCreateTransit()
  {
    //when
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));

    //then
    var loaded = await TransitService.LoadTransit(transit.Id);
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
    Assert.AreEqual(Transit.Statuses.Draft, loaded.Status);
    Assert.NotNull(loaded.Tariff);
    Assert.AreNotEqual(0, loaded.KmRate);
    Assert.NotNull(loaded.DateTime);
  }

  [Test]
  public async Task CanChangeTransitDestination()
  {
    //given
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));

    //when
    await TransitService.ChangeTransitAddressTo(transit.Id,
      new AddressDto("Polska", "Warszawa", "Mazowiecka", 30));

    //then
    var loaded = await TransitService.LoadTransit(transit.Id);
    Assert.AreEqual(30, loaded.To.BuildingNumber);
    Assert.AreEqual("Mazowiecka", loaded.To.Street);
    Assert.NotNull(loaded.EstimatedPrice);
    Assert.IsNull(loaded.Price);
  }

  [Test]
  public async Task CannotChangeDestinationWhenTransitIsCompleted()
  {
    //given
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      destination);
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);
    //and
    await TransitService.AcceptTransit(driver, transit.Id);
    //and
    await TransitService.StartTransit(driver, transit.Id);
    //and
    await TransitService.CompleteTransit(driver, transit.Id, destination);

    //expect
    await TransitService.Awaiting(s => s.ChangeTransitAddressTo(transit.Id,
        new AddressDto("Polska", "Warszawa", "Żytnia", 23)))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CanChangePickupPlace()
  {
    //given
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));

    //when
    await TransitService.ChangeTransitAddressFrom(transit.Id,
      new AddressDto("Polska", "Warszawa", "Puławska", 28));

    //then
    var loaded = await TransitService.LoadTransit(transit.Id);
    Assert.AreEqual(28, loaded.From.BuildingNumber);
    Assert.AreEqual("Puławska", loaded.From.Street);
  }

  [Test]
  public async Task CannotChangePickupPlaceAfterTransitIsAccepted()
  {
    //given
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      destination);
    //and
    var changedTo = new AddressDto("Polska", "Warszawa", "Żytnia", 27);
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);
    //and
    await TransitService.AcceptTransit(driver, transit.Id);

    //expect
    await TransitService.Awaiting(s => s.ChangeTransitAddressFrom(transit.Id, changedTo))
      .Should().ThrowExactlyAsync<InvalidOperationException>();

    //and
    await TransitService.StartTransit(driver, transit.Id);
    //expect
    await TransitService.Awaiting(s => s.ChangeTransitAddressFrom(transit.Id, changedTo))
      .Should().ThrowExactlyAsync<InvalidOperationException>();

    //and
    await TransitService.CompleteTransit(driver, transit.Id, destination);
    //expect
    await TransitService.Awaiting(s => s.ChangeTransitAddressFrom(transit.Id, changedTo))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CannotChangePickupPlaceMoreThanThreeTimes()
  {
    //given
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    await TransitService.ChangeTransitAddressFrom(transit.Id,
      new AddressDto("Polska", "Warszawa", "Żytnia", 26));
    //and
    await TransitService.ChangeTransitAddressFrom(transit.Id,
      new AddressDto("Polska", "Warszawa", "Żytnia", 27));
    //and
    await TransitService.ChangeTransitAddressFrom(transit.Id,
      new AddressDto("Polska", "Warszawa", "Żytnia", 28));

    //expect
    await TransitService.Awaiting(s => s.ChangeTransitAddressFrom(transit.Id,
        new AddressDto("Polska", "Warszawa", "Żytnia", 29)))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CannotChangePickupPlaceWhenItIsFarWayFromOriginal()
  {
    //given
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));

    //expect
    await TransitService.Awaiting(s => s.ChangeTransitAddressFrom(transit.Id, FarAwayAddress(transit)))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CanCancelTransit()
  {
    //given
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));

    //when
    await TransitService.CancelTransit(transit.Id);

    //then
    var loaded = await TransitService.LoadTransit(transit.Id);
    Assert.AreEqual(Transit.Statuses.Cancelled, loaded.Status);
  }

  [Test]
  public async Task CannotCancelTransitAfterItWasStarted()
  {
    //given
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      destination);
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);
    //and
    await TransitService.AcceptTransit(driver, transit.Id);

    //and
    await TransitService.StartTransit(driver, transit.Id);
    //expect
    await TransitService.Awaiting(s => s.CancelTransit(transit.Id))
      .Should().ThrowExactlyAsync<InvalidOperationException>();

    //and
    await TransitService.CompleteTransit(driver, transit.Id, destination);
    //expect
    await TransitService.Awaiting(s => s.CancelTransit(transit.Id))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CanPublishTransit()
  {
    //given
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    await ANearbyDriver("WU1212");

    //when
    await TransitService.PublishTransit(transit.Id);

    //then
    var loaded = await TransitService.LoadTransit(transit.Id);
    Assert.AreEqual(Transit.Statuses.WaitingForDriverAssignment, loaded.Status);
    Assert.NotNull(loaded.Published);
  }

  [Test]
  public async Task CanAcceptTransit()
  {
    //given
    var transit = await RequestTransitFromTo(new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);

    //when
    await TransitService.AcceptTransit(driver, transit.Id);

    //then
    var loaded = await TransitService.LoadTransit(transit.Id);
    Assert.AreEqual(Transit.Statuses.TransitToPassenger, loaded.Status);
    Assert.NotNull(loaded.AcceptedAt);
  }

  [Test]
  public async Task OnlyOneDriverCanAcceptTransit()
  {
    //given
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    var secondDriver = await ANearbyDriver("DW MARIO");
    //and
    await TransitService.PublishTransit(transit.Id);
    //and
    await TransitService.AcceptTransit(driver, transit.Id);

    //expect
    await TransitService.Awaiting(s => s.AcceptTransit(secondDriver, transit.Id))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task TransitCannotBeAcceptedByDriverWhoAlreadyRejected()
  {
    //given
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);

    //and
    await TransitService.RejectTransit(driver, transit.Id);

    //expect
    await TransitService.Awaiting(s => s.AcceptTransit(driver, transit.Id))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task TransitCannotBeAcceptedByDriverWhoHasNotSeenProposal()
  {
    //given
    var transit = await RequestTransitFromTo(new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    var farAwayDriver = await AFarAwayDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);

    //expect
    await TransitService.Awaiting(s => s.AcceptTransit(farAwayDriver, transit.Id))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CanStartTransit()
  {
    //given
    var transit = await RequestTransitFromTo(new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);
    //and
    await TransitService.AcceptTransit(driver, transit.Id);
    //when
    await TransitService.StartTransit(driver, transit.Id);

    //then
    var loaded = await TransitService.LoadTransit(transit.Id);
    Assert.AreEqual(Transit.Statuses.InTransit, loaded.Status);
    Assert.NotNull(loaded.Started);
  }

  [Test]
  public async Task CannotStartNotAcceptedTransit()
  {
    //given
    var transit = await RequestTransitFromTo(new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);

    //expect
    await TransitService.Awaiting(s => s.StartTransit(driver, transit.Id))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Test]
  public async Task CanCompleteTransit()
  {
    //given
    var destination = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      destination);
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);
    //and
    await TransitService.AcceptTransit(driver, transit.Id);
    //and
    await TransitService.StartTransit(driver, transit.Id);

    //when
    await TransitService.CompleteTransit(driver, transit.Id, destination);

    //then
    var loaded = await TransitService.LoadTransit(transit.Id);
    Assert.AreEqual(Transit.Statuses.Completed, loaded.Status);
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
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      addressTo);
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);
    //and
    await TransitService.AcceptTransit(driver, transit.Id);

    //expect
    await TransitService.Awaiting(s => s.CompleteTransit(driver, transit.Id, addressTo))
      .Should().ThrowExactlyAsync<ArgumentException>();
  }

  [Test]
  public async Task CanRejectTransit()
  {
    //given
    var transit = await RequestTransitFromTo(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);

    //when
    await TransitService.RejectTransit(driver, transit.Id);

    //then
    var loaded = await TransitService.LoadTransit(transit.Id);
    Assert.AreEqual(Transit.Statuses.WaitingForDriverAssignment, loaded.Status);
    Assert.IsNull(loaded.AcceptedAt);
  }

  private AddressDto FarAwayAddress(Transit t)
  {
    var addressDto = new AddressDto("Dania", "Kopenhaga", "Mylve", 2);
    GeocodingService.GeocodeAddress(Arg.Is<Address>(address => !(address == t.From)))
      .Returns(new double[] { 1000, 1000 });
    GeocodingService.GeocodeAddress(t.From)
      .Returns(new double[] { 1, 1 });
    return addressDto;
  }

  private async Task<long?> ANearbyDriver(string plateNumber)
  {
    var driver = await Fixtures.ADriver();
    await Fixtures.DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);
    await DriverSessionService.LogIn(driver.Id, plateNumber, CarType.CarClasses.Van, "BRAND");
    await DriverTrackingService.RegisterPosition(driver.Id, 1, 1);
    return driver.Id;
  }

  private async Task<long?> AFarAwayDriver(string plateNumber)
  {
    var driver = await Fixtures.ADriver();
    await Fixtures.DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);
    await DriverSessionService.LogIn(driver.Id, plateNumber, CarType.CarClasses.Van, "BRAND");
    await DriverTrackingService.RegisterPosition(driver.Id, 1000, 1000);
    return driver.Id;
  }

  private async Task<Transit> RequestTransitFromTo(AddressDto pickup, AddressDto destination)
  {
    var aTransitDto = await Fixtures.ATransitDto(
      pickup,
      destination);
    return await TransitService.CreateTransit(aTransitDto);
  }
}