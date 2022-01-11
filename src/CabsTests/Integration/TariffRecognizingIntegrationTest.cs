using LegacyFighter.Cabs.Controllers;
using LegacyFighter.CabsTests.Common;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

public class TariffRecognizingIntegrationTest
{
  private Fixtures Fixtures => _app.Fixtures;
  private TransitController TransitController => _app.TransitController;
  private CabsApp _app = default!;

  [SetUp]
  public void InitializeApp()
  {
    _app = CabsApp.CreateInstance();
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task NewYearsEveTariffShouldBeDisplayed()
  {
    //given
    var transit = await Fixtures.ACompletedTransitAt(60, new LocalDateTime(2021, 12, 31, 8, 30).InUtc().ToInstant());

    //when
    var transitDto = await TransitController.GetTransit(transit.Id);

    //then
    Assert.AreEqual("Sylwester", transitDto.Tariff);
    Assert.AreEqual(3.5f, transitDto.KmRate);

  }

  [Test]
  public async Task WeekendTariffShouldBeDisplayed()
  {
    //given
    var transit = await Fixtures.ACompletedTransitAt(60, new LocalDateTime(2021, 4, 17, 8, 30).InUtc().ToInstant());

    //when
    var transitDto = await TransitController.GetTransit(transit.Id);

    //then
    Assert.AreEqual("Weekend", transitDto.Tariff);
    Assert.AreEqual(1.5f, transitDto.KmRate);
  }

  [Test]
  public async Task WeekendPlusTariffShouldBeDisplayed()
  {
    //given
    var transit = await Fixtures.ACompletedTransitAt(60, new LocalDateTime(2021, 4, 17, 22, 30).InUtc().ToInstant());

    //when
    var transitDto = await TransitController.GetTransit(transit.Id);

    //then
    Assert.AreEqual("Weekend+", transitDto.Tariff);
    Assert.AreEqual(2.5f, transitDto.KmRate);
  }

  [Test]
  public async Task StandardTariffShouldBeDisplayed()
  {
    //given
    var transit = await Fixtures.ACompletedTransitAt(60, new LocalDateTime(2021, 4, 13, 22, 30).InUtc().ToInstant());

    //when
    var transitDto = await TransitController.GetTransit(transit.Id);

    //then
    Assert.AreEqual("Standard", transitDto.Tariff);
    Assert.AreEqual(1.0f, transitDto.KmRate);
  }
}