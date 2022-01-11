using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using LegacyFighter.CabsTests.Common;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

public class CalculateDriverFeeIntegrationTest
{
  private CabsApp _app = default!;
  private IDriverFeeService DriverFeeService => _app.DriverFeeService;
  private IDriverFeeRepository FeeRepository => _app.DriverFeeRepository;
  private ITransitRepository TransitRepository => _app.TransitRepository;
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
  public async Task ShouldCalculateDriversFlatFee()
  {
    //given
    var driver = await ADriver();
    //and
    var transit = await ATransit(driver, 60);
    //and
    await DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);

    //when
    var fee = await DriverFeeService.CalculateDriverFee(transit.Id);

    //then
    Assert.AreEqual(50, fee);
  }

  [Test]
  public async Task ShouldCalculateDriversPercentageFee()
  {
    //given
    var driver = await ADriver();
    //and
    var transit = await ATransit(driver, 80);
    //and
    await DriverHasFee(driver, DriverFee.FeeTypes.Percentage, 50);

    //when
    var fee = await DriverFeeService.CalculateDriverFee(transit.Id);

    //then
    Assert.AreEqual(40, fee);
  }

  [Test]
  public async Task ShouldUseMinimumFee()
  {
    //given
    var driver = await ADriver();
    //and
    var transit = await ATransit(driver, 10);
    //and
    await DriverHasFee(driver, DriverFee.FeeTypes.Percentage, 7, 5);

    //when
    var fee = await DriverFeeService.CalculateDriverFee(transit.Id);

    //then
    Assert.AreEqual(5, fee);
  }

  private Task<DriverFee> DriverHasFee(Driver driver, DriverFee.FeeTypes feeType, int amount, int min)
  {
    var driverFee = new DriverFee
    {
      Driver = driver,
      Amount = amount,
      FeeType = feeType,
      Min = min
    };
    return FeeRepository.Save(driverFee);
  }

  private Task<DriverFee> DriverHasFee(Driver driver, DriverFee.FeeTypes feeType, int amount)
  {
    return DriverHasFee(driver, feeType, amount, 0);
  }

  private Task<Driver> ADriver()
  {
    return DriverService.CreateDriver("FARME100165AB5EW", "Kowalsi", "Janusz", Driver.Types.Regular,
      Driver.Statuses.Active, "");
  }

  private Task<Transit> ATransit(Driver driver, int price)
  {
    var transit = new Transit
    {
      Price = price,
      Driver = driver,
      DateTime = new LocalDate(2020, 10, 20).AtStartOfDayInZone(DateTimeZone.Utc).ToInstant()
    };
    return TransitRepository.Save(transit);
  }
}