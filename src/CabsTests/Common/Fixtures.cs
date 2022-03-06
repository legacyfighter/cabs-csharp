using System.Linq;
using LegacyFighter.Cabs.DistanceValue;
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
  private readonly IClaimService _claimService;
  private readonly IAwardsService _awardsService;
  private readonly ITransitService _transitService;
  private readonly IDriverSessionService _driverSessionService;
  private readonly IDriverTrackingService _driverTrackingService;
  private readonly IDriverAttributeRepository _driverAttributeRepository;

  public Fixtures(
    ITransitRepository transitRepository,
    IDriverFeeRepository feeRepository,
    IClientRepository clientRepository,
    AddressRepository addressRepository,
    IDriverService driverService,
    ICarTypeService carTypeService,
    IClaimService claimService,
    IAwardsService awardsService,
    ITransitService transitService,
    IDriverSessionService driverSessionService,
    IDriverTrackingService driverTrackingService,
    IDriverAttributeRepository driverAttributeRepository)
  {
    _transitRepository = transitRepository;
    _feeRepository = feeRepository;
    _driverService = driverService;
    _carTypeService = carTypeService;
    _claimService = claimService;
    _awardsService = awardsService;
    _clientRepository = clientRepository;
    _addressRepository = addressRepository;
    _driverAttributeRepository = driverAttributeRepository;
    _transitService = transitService;
    _driverSessionService = driverSessionService;
    _driverTrackingService = driverTrackingService;
  }

  public Task<Client> AClient()
  {
    return _clientRepository.Save(new Client());
  }

  public Task<Client> AClient(Client.Types type) 
  {
    var client = new Client
    {
      Type = type
    };
    return _clientRepository.Save(client);
  }

  public async Task<Transit> ATransit(Driver driver, int price, LocalDateTime when, Client? client)
  {
    var transit = new Transit(null, null, client, null, when.InUtc().ToInstant(), Distance.Zero)
    {
      Price = new Money(price)
    };
    transit.ProposeTo(driver);
    transit.AcceptBy(driver, SystemClock.Instance.GetCurrentInstant());
    return await _transitRepository.Save(transit);
  }

  public async Task<Transit> ATransit(Money price) 
  {
    return await ATransit(await ADriver(), price.IntValue);
  }

  public async Task<Transit> ATransit(Driver driver, int price, LocalDateTime when)
  {
    return await ATransit(driver, price, when, null);
  }

  public Task<Transit> ATransit(Driver driver, int price)
  {
    return ATransit(driver, price, SystemClock.Instance.InBclSystemDefaultZone().GetCurrentLocalDateTime(), null);
  }

  public async Task<DriverFee> DriverHasFee(Driver driver, DriverFee.FeeTypes feeType, int amount, int min)
  {
    var driverFee = new DriverFee
    {
      Driver = driver,
      Amount = amount,
      FeeType = feeType,
      Min = new Money(min)
    };
    return await _feeRepository.Save(driverFee);
  }

  public Task<DriverFee> DriverHasFee(Driver driver, DriverFee.FeeTypes feeType, int amount)
  {
    return DriverHasFee(driver, feeType, amount, 0);
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

  public async Task<Driver> ANearbyDriver(string plateNumber) 
  {
    var driver = await ADriver();
    await DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);
    await _driverSessionService.LogIn(driver.Id, plateNumber, CarType.CarClasses.Van, "BRAND");
    await _driverTrackingService.RegisterPosition(driver.Id, 1, 1, SystemClock.Instance.GetCurrentInstant());
    return driver;
  }

  public async Task<Transit> ARequestedAndCompletedTransit(
    int price,
    Instant publishedAt,
    Instant completedAt,
    Client client,
    Driver driver,
    Address from,
    Address destination)
  {
    from = await _addressRepository.Save(from);
    destination = await _addressRepository.Save(destination);
    var transit = new Transit(
      from,
      destination,
      client,
      null,
      publishedAt,
      Distance.Zero);
    transit.PublishAt(publishedAt);
    transit.ProposeTo(driver);
    transit.AcceptBy(driver, publishedAt);
    transit.Start(publishedAt);
    transit.CompleteTransitAt(completedAt, destination, Distance.OfKm(1));
    transit.Price = new Money(price);
    return await _transitRepository.Save(transit);
  }

  public async Task<Transit> ACompletedTransitAt(int price, Instant when)
  {
    var client = await AClient();
    var driver = await ADriver();
    return await ACompletedTransitAt(price, when, client, driver);
  }

  public async Task<Transit> ACompletedTransitAt(int price, Instant publishedAt, Instant completedAt, Client client, Driver driver)
  {
    var destination = new Address("Polska", "Warszawa", "Zytnia", 20);
    var from = new Address("Polska", "Warszawa", "M³ynarska", 20);
    
    return await ARequestedAndCompletedTransit(price, publishedAt, completedAt, client, driver, from, destination);
  }

  public async Task<Transit> ACompletedTransitAt(int price, Instant publishedAt, Client client, Driver driver) 
  {
    return await ACompletedTransitAt(price, publishedAt, publishedAt.Plus(Duration.FromMinutes(10)), client, driver);
  }

  public async Task<Transit> ARequestedAndCompletedTransit(
    int price,
    Instant publishedAt,
    Instant completedAt,
    Client client,
    Driver driver,
    Address from,
    Address destination,
    IClock clock)
  {
    from = await _addressRepository.Save(from);
    destination = await _addressRepository.Save(destination);

    clock.GetCurrentInstant().Returns(publishedAt);
    var transit = await _transitService.CreateTransit(client.Id, from, destination, CarType.CarClasses.Van);
    await _transitService.PublishTransit(transit.Id);
    await _transitService.FindDriversForTransit(transit.Id);
    await _transitService.AcceptTransit(driver.Id, transit.Id);
    await _transitService.StartTransit(driver.Id, transit.Id);
    clock.GetCurrentInstant().Returns(completedAt);
    await _transitService.CompleteTransit(driver.Id, transit.Id, destination);

    return await _transitRepository.Find(transit.Id);
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

  public async Task ClientHasDoneTransits(Client client, int noOfTransits) 
  {
    await Task.WhenAll(Enumerable.Range(1, noOfTransits)
      .Select(async i =>
      {
        var driver = await ADriver();
        var completedTransit = await ACompletedTransitAt(10, SystemClock.Instance.GetCurrentInstant(), client, driver);
        await _transitRepository.Save(completedTransit);
      }));
  }

  public async Task<Claim> CreateClaim(Client client, Transit transit)
  {
    var claimDto = ClaimDto("Okradli mnie na hajs", "$$$", client.Id, transit.Id);
    claimDto.IsDraft = false;
    return await _claimService.Create(claimDto);
  }

  public async Task<Claim> CreateClaim(Client client, Transit transit, string reason) 
  {
    var claimDto = ClaimDto("Okradli mnie na hajs", reason, client.Id, transit.Id);
    claimDto.IsDraft = false;
    return await _claimService.Create(claimDto);
  }

  public async Task<Claim> CreateAndResolveClaim(Client client, Transit transit) 
  {
    var claim = await CreateClaim(client, transit);
    claim = await _claimService.TryToResolveAutomatically(claim.Id);
    return claim;
  }

  public ClaimDto ClaimDto(string desc, string reason, long? clientId, long? transitId) 
  {
    var claimDto = new ClaimDto
    {
      ClientId = clientId,
      TransitId = transitId,
      IncidentDescription = desc,
      Reason = reason
    };
    return claimDto;
  }

  public async Task ClientHasDoneClaims(Client client, int howMany)
  {
    await Task.WhenAll(Enumerable.Range(1, howMany)
      .Select(async i =>
      {
        var driver = await ADriver();
        var transit = await ATransit(driver, 20, SystemClock.Instance.InBclSystemDefaultZone().GetCurrentLocalDateTime(), client);
        await CreateAndResolveClaim(client, transit);
      }));
  }

  public async Task<Client> AClientWithClaims(Client.Types type, int howManyClaims)
  {
    var client = await AClient(type);
    await ClientHasDoneClaims(client, howManyClaims);
    return client;
  }

  public async Task AwardsAccount(Client client) 
  {
    await _awardsService.RegisterToProgram(client.Id);
  }

  public async Task ActiveAwardsAccount(Client client)
  {
    await AwardsAccount(client);
    await _awardsService.ActivateAccount(client.Id);
  }

  public async Task DriverHasAttribute(Driver driver, DriverAttribute.DriverAttributeNames name, string value)
  {
    await _driverAttributeRepository.Save(new DriverAttribute(driver, name, value));
  }
}