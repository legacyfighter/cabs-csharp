using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using NodaTime;
using NodaTime.Extensions;

namespace LegacyFighter.CabsTests.Common;

public class Fixtures
{
  private readonly ITransitRepository _transitRepository;
  private readonly IDriverFeeRepository _feeRepository;
  private readonly IDriverService _driverService;

  public Fixtures(
    ITransitRepository transitRepository,
    IDriverFeeRepository feeRepository,
    IDriverService driverService)
  {
    _transitRepository = transitRepository;
    _feeRepository = feeRepository;
    _driverService = driverService;
  }

  public Task<Transit> ATransit(Driver driver, int price, LocalDateTime when)
  {
    var transit = new Transit
    {
      Price = new Money(price),
      Driver = driver,
      DateTime = when.InUtc().ToInstant()
    };
    return _transitRepository.Save(transit);
  }

  public Task<Transit> ATransit(Driver driver, int price)
  {
    return ATransit(driver, price, SystemClock.Instance.InBclSystemDefaultZone().GetCurrentLocalDateTime());
  }

  public Task<DriverFee> DriverHasFee(Driver driver, DriverFee.FeeTypes feeType, int amount, int min)
  {
    var driverFee = new DriverFee
    {
      Driver = driver,
      Amount = amount,
      FeeType = feeType,
      Min = new Money(min)
    };
    return _feeRepository.Save(driverFee);
  }

  public Task<DriverFee> DriverHasFee(Driver driver, DriverFee.FeeTypes feeType, int amount)
  {
    return DriverHasFee(driver, feeType, amount, 0);
  }

  public Task<Driver> ADriver()
  {
    return _driverService.CreateDriver("FARME100165AB5EW", "Kowalsi", "Janusz", Driver.Types.Regular,
      Driver.Statuses.Active, "");
  }
}