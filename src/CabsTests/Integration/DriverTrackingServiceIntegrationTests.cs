using LegacyFighter.Cabs.Service;
using LegacyFighter.CabsTests.Common;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

public class DriverTrackingServiceIntegrationTest
{
  private static readonly Instant Noon = new LocalDateTime(1989, 12, 12, 12, 12).InUtc().ToInstant();
  private static readonly Instant NoonFive = Noon.Plus(Duration.FromMinutes(5));

  private CabsApp _app = default!;
  private Fixtures Fixtures => _app.Fixtures;
  private IDriverTrackingService DriverTrackingService => _app.DriverTrackingService;
  private IClock Clock { get; set; } = default!;

  [SetUp]
  public void InitializeApp()
  {
    Clock = Substitute.For<IClock>();
    _app = CabsApp.CreateInstance(ctx => ctx.AddSingleton(Clock));
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task CanCalculateTravelledDistanceFromShortTransit()
  {
    //given
    var driver = await Fixtures.ADriver();
    //and
    ItIsNoon();
    //and
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221, Noon);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.31861111111111, -1.6997222222222223, Noon);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221, Noon);

    //when
    var distance = await DriverTrackingService.CalculateTravelledDistance(driver.Id, Noon, NoonFive);

    //then
    Assert.AreEqual("4.009km", distance.PrintIn("km"));
  }

  private void ItIsNoon()
  {
    Clock.GetCurrentInstant().Returns(Noon);
  }
}
