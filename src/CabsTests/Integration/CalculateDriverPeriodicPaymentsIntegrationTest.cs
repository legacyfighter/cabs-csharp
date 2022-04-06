using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.CabsTests.Common;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

public class CalculateDriverPeriodicPaymentsIntegrationTest
{
  private CabsApp _app = default!;
  private Fixtures Fixtures => _app.Fixtures;
  private IDriverService DriverService => _app.DriverService;

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
  public async Task CalculateMonthlyPayment()
  {
    _app.StartReuseRequestScope();
    //given
    var driver = await Fixtures.ADriver();
    //and
    await Fixtures.TransitDetails(driver, 60, new LocalDateTime(2000, 10, 1, 6, 30));
    await Fixtures.TransitDetails(driver, 70, new LocalDateTime(2000, 10, 10, 2, 30));
    await Fixtures.TransitDetails(driver, 80, new LocalDateTime(2000, 10, 30, 6, 30));
    await Fixtures.TransitDetails(driver, 60, new LocalDateTime(2000, 11, 10, 1, 30));
    await Fixtures.TransitDetails(driver, 30, new LocalDateTime(2000, 11, 10, 1, 30));
    await Fixtures.TransitDetails(driver, 15, new LocalDateTime(2000, 12, 10, 2, 30));
    //and
    await Fixtures.DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);
    _app.EndReuseRequestScope();

    //when
    var feeOctober = await DriverService.CalculateDriverMonthlyPayment(driver.Id, 2000, 10);
    //then
    Assert.AreEqual(new Money(180), feeOctober);

    //when
    var feeNovember = await DriverService.CalculateDriverMonthlyPayment(driver.Id, 2000, 11);
    //then
    Assert.AreEqual(new Money(70), feeNovember);

    //when
    var feeDecember = await DriverService.CalculateDriverMonthlyPayment(driver.Id, 2000, 12);
    //then
    Assert.AreEqual(new Money(5), feeDecember);
  }

  [Test]
  public async Task CalculateYearlyPayment()
  {
    _app.StartReuseRequestScope();
    //given
    var driver = await Fixtures.ADriver();
    //and
    await Fixtures.TransitDetails(driver, 60, new LocalDateTime(2000, 10, 1, 6, 30));
    await Fixtures.TransitDetails(driver, 70, new LocalDateTime(2000, 10, 10, 2, 30));
    await Fixtures.TransitDetails(driver, 80, new LocalDateTime(2000, 10, 30, 6, 30));
    await Fixtures.TransitDetails(driver, 60, new LocalDateTime(2000, 11, 10, 1, 30));
    await Fixtures.TransitDetails(driver, 30, new LocalDateTime(2000, 11, 10, 1, 30));
    await Fixtures.TransitDetails(driver, 15, new LocalDateTime(2000, 12, 10, 2, 30));
    //and
    await Fixtures.DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);
    _app.EndReuseRequestScope();

    //when
    var payments = await DriverService.CalculateDriverYearlyPayment(driver.Id, 2000);

    //then
    Assert.AreEqual(new Money(0), payments[Month.January]);
    Assert.AreEqual(new Money(0), payments[Month.February]);
    Assert.AreEqual(new Money(0), payments[Month.March]);
    Assert.AreEqual(new Money(0), payments[Month.April]);
    Assert.AreEqual(new Money(0), payments[Month.May]);
    Assert.AreEqual(new Money(0), payments[Month.June]);
    Assert.AreEqual(new Money(0), payments[Month.July]);
    Assert.AreEqual(new Money(0), payments[Month.August]);
    Assert.AreEqual(new Money(0), payments[Month.September]);
    Assert.AreEqual(new Money(180), payments[Month.October]);
    Assert.AreEqual(new Money(70), payments[Month.November]);
    Assert.AreEqual(new Money(5), payments[Month.December]);
  }
}