using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Service;
using LegacyFighter.CabsTests.Common;

namespace LegacyFighter.CabsTests.Integration;

public class CalculateDriverFeeIntegrationTest
{
  private CabsApp _app = default!;
  private Fixtures Fixtures => _app.Fixtures;
  private IDriverFeeService DriverFeeService => _app.DriverFeeService;

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
  public async Task ShouldCalculateDriversFlatFee()
  {
    //given
    var driver = await Fixtures.ADriver();
    //and
    var transit = await Fixtures.ATransit(driver, 60);
    //and
    await Fixtures.DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);

    //when
    var fee = await DriverFeeService.CalculateDriverFee(transit.Id);

    //then
    Assert.AreEqual(new Money(50), fee);
  }

  [Test]
  public async Task ShouldCalculateDriversPercentageFee()
  {
    //given
    var driver = await Fixtures.ADriver();
    //and
    var transit = await Fixtures.ATransit(driver, 80);
    //and
    await Fixtures.DriverHasFee(driver, DriverFee.FeeTypes.Percentage, 50);

    //when
    var fee = await DriverFeeService.CalculateDriverFee(transit.Id);

    //then
    Assert.AreEqual(new Money(40), fee);
  }

  [Test]
  public async Task ShouldUseMinimumFee()
  {
    //given
    var driver = await Fixtures.ADriver();
    //and
    var transit = await Fixtures.ATransit(driver, 10);
    //and
    await Fixtures.DriverHasFee(driver, DriverFee.FeeTypes.Percentage, 7, 5);

    //when
    var fee = await DriverFeeService.CalculateDriverFee(transit.Id);

    //then
    Assert.AreEqual(new Money(5), fee);
  }

}