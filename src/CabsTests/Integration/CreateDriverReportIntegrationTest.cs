using System.Linq;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Crm.Claims;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.DriverFleet.DriverReports;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.Service;
using LegacyFighter.Cabs.Tracking;
using LegacyFighter.CabsTests.Common;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

public class CreateDriverReportIntegrationTest
{
  private static readonly Instant DayBeforeYesterday = new LocalDateTime(1989, 12, 12, 12, 12).InUtc().ToInstant();
  private static readonly Instant Yesterday = DayBeforeYesterday.Plus(Duration.FromDays(1));
  private static readonly Instant Today = Yesterday.Plus(Duration.FromDays(1));

  private Fixtures Fixtures => _app.Fixtures;
  private CabsApp _app = default!;
  private IClock Clock { get; set; } = default!;
  private IGeocodingService GeocodingService { get; set; } = default!;
  private ITransitService TransitService => _app.TransitService;
  private IDriverTrackingService DriverTrackingService => _app.DriverTrackingService;
  private IDriverSessionService DriverSessionService => _app.DriverSessionService;
  private ICarTypeService CarTypeService => _app.CarTypeService;
  private DriverReportController DriverReportController => _app.DriverReportController;
  private IAddressRepository AddressRepository => _app.AddressRepository;
  private IClaimService ClaimService => _app.ClaimService;

  [SetUp]
  public async Task InitializeApp()
  {
    Clock = Substitute.For<IClock>();
    GeocodingService = Substitute.For<IGeocodingService>();
    _app = CabsApp.CreateInstance(ctx =>
    {
      ctx.AddSingleton(GeocodingService);
      ctx.AddSingleton(Clock);
    });
    await Fixtures.AnActiveCarCategory(CarClasses.Van);
    await Fixtures.AnActiveCarCategory(CarClasses.Premium);
  }

  [TearDown]
  public void DisposeOfApp()
  {
    _app.Dispose();
  }

  [Test]
  public async Task ShouldCreateDriversReport()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    var driver = await ADriver(Driver.Statuses.Active, "JAN", "NOWAK", "FARME100165AB5EW");
    //and
    await Fixtures.DriverHasAttribute(driver, DriverAttributeNames.CompanyName, "UBER");
    await Fixtures.DriverHasAttribute(driver, DriverAttributeNames.PenaltyPoints, "21");
    await Fixtures.DriverHasAttribute(driver, DriverAttributeNames.MedicalExaminationRemarks,
      "private info");
    //and
    await DriverHasDoneSessionAndPicksSomeoneUpInCar(driver, client, CarClasses.Van, "WU1213", "SCODA FABIA",
      Today);
    await DriverHasDoneSessionAndPicksSomeoneUpInCar(driver, client, CarClasses.Van, "WU1213", "SCODA OCTAVIA",
      Yesterday);
    var inBmw = await DriverHasDoneSessionAndPicksSomeoneUpInCar(driver, client, CarClasses.Van, "WU1213",
      "BMW M2", DayBeforeYesterday);
    //and
    await Fixtures.CreateClaim(client, inBmw, "za szybko");

    //when
    var driverReportWithin2days = await LoadReportIncludingPastDays(driver, 2);
    var driverReportWithin1day = await LoadReportIncludingPastDays(driver, 1);
    var driverReportForJustToday = await LoadReportIncludingPastDays(driver, 0);

    //then
    Assert.AreEqual(3, driverReportWithin2days.Sessions.Keys.Count);
    Assert.AreEqual(2, driverReportWithin1day.Sessions.Keys.Count);
    Assert.AreEqual(1, driverReportForJustToday.Sessions.Keys.Count);


    Assert.AreEqual("FARME100165AB5EW", driverReportWithin2days.DriverDto.DriverLicense);
    Assert.AreEqual("JAN", driverReportWithin2days.DriverDto.FirstName);
    Assert.AreEqual("NOWAK", driverReportWithin2days.DriverDto.LastName);
    Assert.AreEqual(2, driverReportWithin2days.Attributes.Count);
    Assert.True(
      driverReportWithin2days.Attributes.Contains(
        new DriverAttributeDto(DriverAttributeNames.CompanyName, "UBER")));
    Assert.True(
      driverReportWithin2days.Attributes.Contains(
        new DriverAttributeDto(DriverAttributeNames.PenaltyPoints, "21")));

    TransitsInSessionIn("SCODA FABIA", driverReportWithin2days).Should().HaveCount(1);
    TransitsInSessionIn("SCODA FABIA", driverReportWithin2days)[0].ClaimDto.Should().BeNull();

    TransitsInSessionIn("SCODA OCTAVIA", driverReportWithin2days).Should().HaveCount(1);
    TransitsInSessionIn("SCODA OCTAVIA", driverReportWithin2days)[0].ClaimDto.Should().BeNull();

    TransitsInSessionIn("BMW M2", driverReportWithin2days).Should().HaveCount(1);
    TransitsInSessionIn("BMW M2", driverReportWithin2days)[0].ClaimDto.Should().NotBeNull();
    TransitsInSessionIn("BMW M2", driverReportWithin2days)[0].ClaimDto.Reason.Should().Be("za szybko");
  }

  private async Task<DriverReport> LoadReportIncludingPastDays(Driver driver, int days)
  {
    Clock.GetCurrentInstant().Returns(Today);
    var driverReport = await DriverReportController.LoadReportForDriver(driver.Id, days);
    return driverReport;
  }

  private System.Collections.Generic.List<TransitDto> TransitsInSessionIn(string carBrand, DriverReport driverReport)
  {
    return driverReport
      .Sessions
      .Where(e => e.Key.CarBrand == carBrand)
      .Select(e => e.Value)
      .SelectMany(dtos => dtos)
      .ToList();
  }

  private async Task<TransitDto> DriverHasDoneSessionAndPicksSomeoneUpInCar(Driver driver, Client client,
    CarClasses carClass, string plateNumber, string carBrand, Instant when)
  {
    Clock.GetCurrentInstant().Returns(when);
    var driverId = driver.Id;
    await DriverSessionService.LogIn(driverId, plateNumber, carClass, carBrand);
    await DriverTrackingService.RegisterPosition(driverId, 10, 20, SystemClock.Instance.GetCurrentInstant());
    var from = await Address("PL", "MAZ", "WAW", "STREET", 1, 10, 20);
    var destination = await Address("PL", "MAZ", "WAW", "STREET", 100, 10.01, 20.01);
    var transit = await TransitService.CreateTransit(client.Id, from, destination, carClass);
    await TransitService.PublishTransit(transit.Id);
    await TransitService.AcceptTransit(driverId, transit.Id);
    await TransitService.StartTransit(driverId, transit.Id);
    var destinationAddress = await Address("PL", "MAZ", "WAW", "STREET", 100, 10.01, 20.01);
    await TransitService.CompleteTransit(driverId, transit.Id, destinationAddress);
    await DriverSessionService.LogOutCurrentSession(driverId);
    return transit;
  }

  private async Task<Driver> ADriver(Driver.Statuses status, string name, string lastName, string driverLicense)
  {
    var driver = await Fixtures.ADriver(status, name, lastName, driverLicense);
    await Fixtures.DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);
    return driver;
  }

  private async Task<Address> Address(string country, string district, string city, string street, int buildingNumber,
    double latitude, double longitude)
  {
    var address = new Address
    {
      Country = country,
      District = district,
      City = city,
      Street = street,
      BuildingNumber = buildingNumber
    };
    GeocodingService.GeocodeAddress(address).Returns(new[] { latitude, longitude });
    return await AddressRepository.Save(address);
  }
}