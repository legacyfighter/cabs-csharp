using System.Linq;
using LegacyFighter.Cabs.Dto;
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
  private readonly IClientRepository _clientRepository;
  private readonly AddressRepository _addressRepository;
  private readonly IDriverService _driverService;
  private readonly ICarTypeService _carTypeService;

  public Fixtures(
    ITransitRepository transitRepository,
    IDriverFeeRepository feeRepository,
    IClientRepository clientRepository,
    AddressRepository addressRepository,
    IDriverService driverService,
    ICarTypeService carTypeService)
  {
    _transitRepository = transitRepository;
    _feeRepository = feeRepository;
    _driverService = driverService;
    _carTypeService = carTypeService;
    _clientRepository = clientRepository;
    _addressRepository = addressRepository;
  }

  public Task<Client> AClient()
  {
    return _clientRepository.Save(new Client());
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

  public async Task<Transit> ACompletedTransitAt(int price, Instant when)
  {
    var transit = await ATransit(null, price);
    transit.DateTime = when;
    transit.To = await _addressRepository.Save(new Address("Polska", "Warszawa", "Zytnia", 20));
    transit.From = await _addressRepository.Save(new Address("Polska", "Warszawa", "M³ynarska", 20));
    transit.Client = await AClient();
    return await _transitRepository.Save(transit);
  }

  public async Task<CarType> AnActiveCarCategory(CarType.CarClasses carClass)
  {
    var carTypeDto = new CarTypeDto
    {
      CarClass = carClass,
      Description = "opis"
    };
    var carType = await _carTypeService.Create(carTypeDto);
    foreach (var _ in Enumerable.Range(1, carType.MinNoOfCarsToActivateClass))
    {
      await _carTypeService.RegisterCar(carType.CarClass);
    }

    await _carTypeService.Activate(carType.Id);
    return carType;
  }

  public TransitDto ATransitDto(Client client, AddressDto from, AddressDto to)
  {
    var transitDto = new TransitDto
    {
      ClientDto = new ClientDto(client),
      From = from,
      To = to
    };
    return transitDto;
  }

  public async Task<TransitDto> ATransitDto(AddressDto from, AddressDto to)
  {
    return ATransitDto(await AClient(), from, to);
  }
}