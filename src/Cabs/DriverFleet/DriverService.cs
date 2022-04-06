using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Ride.Details;
using NodaTime;

namespace LegacyFighter.Cabs.DriverFleet;

public class DriverService : IDriverService
{
  private readonly IDriverRepository _driverRepository;
  private readonly IDriverAttributeRepository _driverAttributeRepository;
  private readonly ITransitDetailsFacade _transitDetailsFacade;
  private readonly IDriverFeeService _driverFeeService;

  public DriverService(
    IDriverRepository driverRepository,
    IDriverAttributeRepository driverAttributeRepository,
    ITransitDetailsFacade transitDetailsFacade,
    IDriverFeeService driverFeeService)
  {
    _driverRepository = driverRepository;
    _driverAttributeRepository = driverAttributeRepository;
    _transitDetailsFacade = transitDetailsFacade;
    _driverFeeService = driverFeeService;
  }

  public async Task<Driver> CreateDriver(string license, string lastName, string firstName, Driver.Types type,
    Driver.Statuses status, string photo)
  {
    var driver = new Driver();
    if (status == Driver.Statuses.Active)
    {
      driver.DriverLicense = DriverLicense.WithLicense(license);
    }
    else
    {
      driver.DriverLicense = DriverLicense.WithoutValidation(license);
    }

    driver.LastName = lastName;
    driver.FirstName = firstName;
    driver.Status = status;
    driver.Type = type;
    if (photo != null && photo.Any())
    {
      if (photo.IsBase64())
      {
        driver.Photo = photo;
      }
      else
      {
        throw new ArgumentException("Illegal photo in base64");
      }
    }

    return await _driverRepository.Save(driver);
  }

  public async Task ChangeLicenseNumber(string newLicense, long? driverId)
  {
    var driver = await _driverRepository.Find(driverId);
    if (driver == null)
    {
      throw new ArgumentException("Driver does not exists, id = " + driverId);
    }

    driver.DriverLicense = DriverLicense.WithLicense(newLicense);

    if (driver.Status != Driver.Statuses.Active)
    {
      throw new InvalidOperationException("Driver is not active, cannot change license");
    }
  }

  public async Task ChangeDriverStatus(long? driverId, Driver.Statuses status)
  {
    var driver = await _driverRepository.Find(driverId);
    if (driver == null)
    {
      throw new ArgumentException("Driver does not exists, id = " + driverId);
    }

    if (status == Driver.Statuses.Active)
    {
      try
      {
        driver.DriverLicense = DriverLicense.WithLicense(driver.DriverLicense.ValueAsString);
      }
      catch (ArgumentException e)
      {
        throw new InvalidOperationException(e.Message, e);
      }
    }

    driver.Status = status;
  }

  public async Task ChangePhoto(long driverId, string photo)
  {
    var driver = await _driverRepository.Find(driverId);
    if (driver == null)
    {
      throw new ArgumentException("Driver does not exists, id = " + driverId);
    }

    if (photo != null && photo.Any())
    {
      if (photo.IsBase64())
      {
        driver.Photo = photo;
      }
      else
      {
        throw new ArgumentException("Illegal photo in base64");
      }
    }

    driver.Photo = photo;
    await _driverRepository.Save(driver);
  }

  public async Task<Money> CalculateDriverMonthlyPayment(long? driverId, int year, int month) 
  {
    var driver = await _driverRepository.Find(driverId);
    if (driver == null)
    {
      throw new ArgumentException($"Driver does not exists, id = {driverId}");
    }

    var yearMonth = new YearMonth(year, month);
    var from = yearMonth
      .OnDayOfMonth(1).AtStartOfDayInZone(DateTimeZoneProviders.Bcl.GetSystemDefault())
      
      .ToInstant();
    var to = yearMonth

      .AtEndOfMonth().PlusDays(1).AtStartOfDayInZone(DateTimeZoneProviders.Bcl.GetSystemDefault()).ToInstant();

    var transitsList = await _transitDetailsFacade.FindByDriver(driverId, from, to);

    var sum = await transitsList
      .Select(t => _driverFeeService.CalculateDriverFee(t.Price, driverId)).Aggregate(
        Task.FromResult(Money.Zero), 
        async (sumSoFar, next) => await sumSoFar + await next);

    return sum;
  }

  public async Task<Dictionary<Month, Money>> CalculateDriverYearlyPayment(long? driverId, int year)
  {
    var payments = new Dictionary<Month, Money>();
    foreach (var m in Month.Values()) 
    {
      payments[m] = await CalculateDriverMonthlyPayment(driverId, year, m.Value);
    }
    return payments;
  }

  public async Task<DriverDto> LoadDriver(long? driverId)
  {
    var driver = await _driverRepository.Find(driverId);
    if (driver == null)
    {
      throw new ArgumentException("Driver does not exists, id = " + driverId);
    }

    return new DriverDto(driver);
  }

  public async Task<ISet<DriverDto>> LoadDrivers(ICollection<long?> ids) 
  {
    return (await _driverRepository.FindAllById(ids))
      .Select(d => new DriverDto(d)).ToHashSet();
  }

  public async Task AddAttribute(long driverId, DriverAttributeNames attr, string value)
  {
    var driver = await _driverRepository.Find(driverId);
    if (driver == null)
    {
      throw new ArgumentException("Driver does not exists, id = " + driverId);
    }

    await _driverAttributeRepository.Save(new DriverAttribute(driver, attr, value));
  }

  public async Task<bool> Exists(long? driverId)
  {
    return (await _driverRepository.FindById(driverId)).HasValue;
  }

  public async Task MarkOccupied(long? driverId)
  {
    var driver = await _driverRepository.Find(driverId);
    driver.Occupied = true;
  }

  public async Task MarkNotOccupied(long? driverId)
  {
    var driver = await _driverRepository.Find(driverId);
    driver.Occupied = false;
  }
}