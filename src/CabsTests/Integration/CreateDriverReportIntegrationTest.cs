using System.Linq;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.DriverFleet.DriverReports;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Ride;
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
  private RideFixture RiderFixture => _app.RideFixture;
  private CabsApp _app = default!;
  private DriverReportController DriverReportController => _app.DriverReportController;
  private IGeocodingService GeocodingService { get; set; } = default!;
  private IClock Clock { get; set; } = default!;

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
    _app.StartReuseRequestScope();
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
    await RiderFixture.DriverHasDoneSessionAndPicksSomeoneUpInCar(
      driver, client, CarClasses.Van, "WU1213", "SCODA FABIA", Today, GeocodingService, Clock);
    await RiderFixture.DriverHasDoneSessionAndPicksSomeoneUpInCar(
      driver, client, CarClasses.Van, "WU1213", "SCODA OCTAVIA", Yesterday, GeocodingService, Clock);
    var inBmw = await RiderFixture.DriverHasDoneSessionAndPicksSomeoneUpInCar(
      driver, client, CarClasses.Van, "WU1213", "BMW M2", DayBeforeYesterday, GeocodingService, Clock);
    //and
    await Fixtures.CreateClaim(client, inBmw, "za szybko");
    _app.EndReuseRequestScope();

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

  private async Task<Driver> ADriver(Driver.Statuses status, string name, string lastName, string driverLicense)
  {
    var driver = await Fixtures.ADriver(status, name, lastName, driverLicense);
    await Fixtures.DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);
    return driver;
  }
}