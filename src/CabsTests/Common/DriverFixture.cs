using System;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Tracking;
using NodaTime;

namespace LegacyFighter.CabsTests.Common;

public class DriverFixture
{
  private readonly IDriverFeeRepository _feeRepository;
  private readonly IDriverService _driverService;
  private readonly IDriverSessionService _driverSessionService;
  private readonly IDriverTrackingService _driverTrackingService;
  private readonly IDriverAttributeRepository _driverAttributeRepository;

  public DriverFixture(
    IDriverFeeRepository feeRepository,
    IDriverService driverService,
    IDriverSessionService driverSessionService,
    IDriverTrackingService driverTrackingService, 
    IDriverAttributeRepository driverAttributeRepository)
  {
    _feeRepository = feeRepository;
    _driverService = driverService;
    _driverSessionService = driverSessionService;
    _driverTrackingService = driverTrackingService;
    _driverAttributeRepository = driverAttributeRepository;
  }

  public async Task<DriverFee> DriverHasFee(Driver driver, DriverFee.FeeTypes feeType, int amount, int min)
  {
    var driverFee = await _feeRepository.FindByDriverId(driver.Id);
    if (driverFee == null)
    {
      driverFee = new DriverFee();
    }

    driverFee.Driver = driver;
    driverFee.Amount = amount;
    driverFee.FeeType = feeType;
    driverFee.Min = new Money(min);
    return await _feeRepository.Save(driverFee);
  }

  public async Task<DriverFee> DriverHasFee(Driver driver, DriverFee.FeeTypes feeType, int amount)
  {
    return await DriverHasFee(driver, feeType, amount, 0);
  }

  public async Task<Driver> ADriver()
  {
    return await ADriver(Driver.Statuses.Active, "Janusz", "Kowalsi", "FARME100165AB5EW");
  }

  public async Task<Driver> ADriver(Driver.Statuses status, string name, string lastName, string driverLicense)
  {
    return await _driverService.CreateDriver(driverLicense, lastName, name, Driver.Types.Regular,
      status, "");
  }

  public async Task<Driver> ANearbyDriver(IGeocodingService stubbedGeocodingService, Address pickup)
  {
    var random = new Random();
    var latitude = random.NextDouble();
    var longitude = random.NextDouble();
    stubbedGeocodingService.GeocodeAddress(pickup).Returns(new[] { latitude, longitude });
    return await ANearbyDriver(
      "WU DAMIAN",
      latitude,
      longitude,
      CarClasses.Van,
      SystemClock.Instance.GetCurrentInstant(),
      "brand");
  }

  public async Task<Driver> ANearbyDriver(
    IGeocodingService stubbedGeocodingService,
    Address pickup,
    double latitude,
    double longitude)
  {
    stubbedGeocodingService.GeocodeAddress(Arg.Is<Address>(a => new AddressMatcher(pickup).Matches(a)))
      .Returns(new[] { latitude, longitude });
    return await ANearbyDriver("WU DAMIAN", latitude, longitude, CarClasses.Van,
      SystemClock.Instance.GetCurrentInstant(), "brand");
  }

  public async Task<Driver> ANearbyDriver(
    string plateNumber,
    double latitude,
    double longitude,
    CarClasses carClass,
    Instant when,
    string carBrand)
  {
    var driver = await ADriver();
    await DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);
    await DriverLogsIn(plateNumber, carClass, driver, carBrand);
    return await DriverIsAtGeoLocalization(plateNumber, latitude, longitude, carClass, driver, when, carBrand);
  }

  private async Task<Driver> DriverIsAtGeoLocalization(
    string plateNumber,
    double latitude,
    double longitude,
    CarClasses carClass,
    Driver driver,
    Instant when,
    string carBrand)
  {
    await _driverTrackingService.RegisterPosition(driver.Id, latitude, longitude, when);
    return driver;
  }

  private async Task DriverLogsIn(
    string plateNumber,
    CarClasses carClass,
    Driver driver,
    string carBrand)
  {
    await _driverSessionService.LogIn(driver.Id, plateNumber, carClass, carBrand);
  }

  public async Task DriverLogsOut(Driver driver)
  {
    await _driverSessionService.LogOutCurrentSession(driver.Id);
  }

  public async Task DriverHasAttribute(
    Driver driver,
    DriverAttributeNames name,
    string value)
  {
    await _driverAttributeRepository.Save(new DriverAttribute(driver, name, value));
  }
}