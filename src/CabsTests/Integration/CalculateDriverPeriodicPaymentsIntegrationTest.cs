using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using LegacyFighter.CabsTests.Common;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

public class CalculateDriverPeriodicPaymentsIntegrationTest
{
  private IDriverService DriverService => _app.DriverService;
  private ITransitRepository TransitRepository => _app.TransitRepository;
  private IDriverFeeRepository FeeRepository => _app.DriverFeeRepository;
  private AddressRepository AddressRepository => _app.AddressRepository;
  private IClientRepository ClientRepository => _app.ClientRepository;
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
  public async Task CalculateMonthlyPayment()
  {
    //given
    var driver = await ADriver();
    //and
    await ATransit(driver, 60, new LocalDateTime(2000, 10, 1, 6, 30));
    await ATransit(driver, 70, new LocalDateTime(2000, 10, 10, 2, 30));
    await ATransit(driver, 80, new LocalDateTime(2000, 10, 30, 6, 30));
    await ATransit(driver, 60, new LocalDateTime(2000, 11, 10, 1, 30));
    await ATransit(driver, 30, new LocalDateTime(2000, 11, 10, 1, 30));
    await ATransit(driver, 15, new LocalDateTime(2000, 12, 10, 2, 30));
    //and
    await DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);

    //when
    var feeOctober = await DriverService.CalculateDriverMonthlyPayment(driver.Id, 2000, 10);
    //then
    Assert.AreEqual(180, feeOctober);

    //when
    var feeNovember = await DriverService.CalculateDriverMonthlyPayment(driver.Id, 2000, 11);
    //then
    Assert.AreEqual(70, feeNovember);

    //when
    var feeDecember = await DriverService.CalculateDriverMonthlyPayment(driver.Id, 2000, 12);
    //then
    Assert.AreEqual(5, feeDecember);
  }

  [Test]
  public async Task CalculateYearlyPayment()
  {
    //given
    var driver = await ADriver();
    //and
    await ATransit(driver, 60, new LocalDateTime(2000, 10, 1, 6, 30));
    await ATransit(driver, 70, new LocalDateTime(2000, 10, 10, 2, 30));
    await ATransit(driver, 80, new LocalDateTime(2000, 10, 30, 6, 30));
    await ATransit(driver, 60, new LocalDateTime(2000, 11, 10, 1, 30));
    await ATransit(driver, 30, new LocalDateTime(2000, 11, 10, 1, 30));
    await ATransit(driver, 15, new LocalDateTime(2000, 12, 10, 2, 30));
    //and
    await DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);

    //when
    var payments = await DriverService.CalculateDriverYearlyPayment(driver.Id, 2000);

    //then
    Assert.AreEqual(0, payments[Month.January]);
    Assert.AreEqual(0, payments[Month.February]);
    Assert.AreEqual(0, payments[Month.March]);
    Assert.AreEqual(0, payments[Month.April]);
    Assert.AreEqual(0, payments[Month.May]);
    Assert.AreEqual(0, payments[Month.June]);
    Assert.AreEqual(0, payments[Month.July]);
    Assert.AreEqual(0, payments[Month.August]);
    Assert.AreEqual(0, payments[Month.September]);
    Assert.AreEqual(180, payments[Month.October]);
    Assert.AreEqual(70, payments[Month.November]);
    Assert.AreEqual(5, payments[Month.December]);
  }

  private Task<Transit> ATransit(Driver driver, int price, LocalDateTime when)
  {
    var transit = new Transit
    {
      Price = price,
      Driver = driver,
      DateTime = when.InUtc().ToInstant()
    };
    return TransitRepository.Save(transit);
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
}