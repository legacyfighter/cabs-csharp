using System.Linq;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Crm.Claims;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using NodaTime;
using NodaTime.Extensions;

namespace LegacyFighter.CabsTests.Common;

public class Fixtures
{
  private readonly AddressFixture _addressFixture;
  private readonly ClaimFixture _claimFixture;
  private readonly DriverFixture _driverFixture;
  private readonly ClientFixture _clientFixture;
  private readonly TransitFixture _transitFixture;
  private readonly AwardsAccountFixture _awardsAccountFixture;
  private readonly CarTypeFixture _carTypeFixture;
  private readonly RideFixture _rideFixture;

  public Fixtures(
    AddressFixture addressFixture,
    ClaimFixture claimFixture,
    DriverFixture driverFixture,
    ClientFixture clientFixture,
    TransitFixture transitFixture,
    AwardsAccountFixture awardsAccountFixture,
    CarTypeFixture carTypeFixture,
    RideFixture rideFixture)
  {
    _addressFixture = addressFixture;
    _claimFixture = claimFixture;
    _driverFixture = driverFixture;
    _clientFixture = clientFixture;
    _transitFixture = transitFixture;
    _awardsAccountFixture = awardsAccountFixture;
    _carTypeFixture = carTypeFixture;
    _rideFixture = rideFixture;
  }

  public async Task<Address> AnAddress() 
  {
    return await _addressFixture.AnAddress();
  }

  public async Task<AddressDto> AnAddress(
    IGeocodingService geocodingService,
    string country,
    string city,
    string street,
    int buildingNumber)
  {
    return await _addressFixture.AnAddress(
      geocodingService,
      country,
      city,
      street,
      buildingNumber);
  }

  public async Task<Client> AClient()
  {
    return await _clientFixture.AClient();
  }

  public async Task<Client> AClient(Client.Types type)
  {
    return await _clientFixture.AClient(type);
  }

  public async Task<Transit> TransitDetails(Driver driver, int price, LocalDateTime when, Client client) 
  {
    return await _transitFixture.TransitDetails(
      driver,
      price,
      when,
      client,
      await AnAddress(),
      await AnAddress());
  }

  public async Task<Transit> TransitDetails(Driver driver, int price, LocalDateTime when) 
  {
    return await _transitFixture.TransitDetails(
      driver,
      price,
      when,
      await AClient(),
      await AnAddress(),
      await AnAddress());
  }


  public async Task<TransitDto> ATransitDto(AddressDto from, AddressDto to)
  {
    return _transitFixture.ATransitDto(await AClient(), from, to);
  }

  public async Task<DriverFee> DriverHasFee(Driver driver, DriverFee.FeeTypes feeType, int amount, int min)
  {
    return await _driverFixture.DriverHasFee(driver, feeType, amount, min);
  }

  public async Task<DriverFee> DriverHasFee(Driver driver, DriverFee.FeeTypes feeType, int amount)
  {
    return await _driverFixture.DriverHasFee(driver, feeType, amount);
  }

  public async Task<Driver> ADriver()
  {
    return await _driverFixture.ADriver();
  }

  public async Task<Driver> ADriver(Driver.Statuses status, string name, string lastName, string driverLicense)
  {
    return await _driverFixture.ADriver(status, name, lastName, driverLicense);
  }

  public async Task<Driver> ANearbyDriver(IGeocodingService stubbedGeocodingService, Address pickup) 
  {
    return await _driverFixture.ANearbyDriver(stubbedGeocodingService, pickup);
  }

  public async Task<Driver> ANearbyDriver(
    IGeocodingService stubbedGeocodingService,
    Address pickup,
    double latitude,
    double longitude) 
  {
    return await _driverFixture.ANearbyDriver(
      stubbedGeocodingService,
      pickup,
      latitude,
      longitude);
  }

  public async Task<Driver> ANearbyDriver(
    string plateNumber,
    double latitude,
    double longitude,
    CarClasses carClass,
    Instant when) 
  {
    return await _driverFixture.ANearbyDriver(
      plateNumber,
      latitude,
      longitude,
      carClass,
      when,
      "brand");
  }

  public async Task<Driver> ANearbyDriver(
    string plateNumber,
    double latitude,
    double longitude,
    CarClasses carClass,
    Instant when,
    string carBrand) 
  {
    return await _driverFixture.ANearbyDriver(
      plateNumber,
      latitude,
      longitude,
      carClass,
      when,
      carBrand);
  }

  public async Task DriverHasAttribute(Driver driver, DriverAttributeNames name, string value) 
  {
    await _driverFixture.DriverHasAttribute(driver, name, value);
  }

  public async Task<Transit> ARide(int price, Client client, Driver driver, Address from, Address destination) 
  {
    return await _rideFixture.ARide(price, client, driver, from, destination);
  }

  public async Task<Transit> ARideWithFixedClock(int price, Instant publishedAt, Instant completedAt, Client client, Driver driver, Address from, Address destination, IClock clock) 
  {
    return await _rideFixture.ARideWithFixedClock(price, publishedAt, completedAt, client, driver, from, destination, clock);
  }

  public async Task AnActiveCarCategory(CarClasses carClass)
  {
    await _carTypeFixture.AnActiveCarCategory(carClass);
  }

  public async Task ClientHasDoneTransits(Client client, int noOfTransits, IGeocodingService geocodingService) 
  {
    await Task.WhenAll(Enumerable.Range(1, noOfTransits)
      .Select(async i =>
      {
        var pickup = await AnAddress();
        var driver = await ANearbyDriver(geocodingService, pickup);
        await ARide(10, client, driver, pickup, await AnAddress());
      }));
  }

  public async Task<Claim> CreateClaim(Client client, Transit transit)
  {
    return await _claimFixture.CreateClaim(client, transit);
  }

  public async Task<Claim> CreateClaim(Client client, TransitDto transitDto, string reason) 
  {
    return await _claimFixture.CreateClaim(client, transitDto, reason);
  }

  public async Task<Claim> CreateAndResolveClaim(Client client, Transit transit)
  {
    return await _claimFixture.CreateAndResolveClaim(client, transit);
  }

  public async Task ClientHasDoneClaimsAfterCompletedTransit(Client client, int howMany)
  {
    await Task.WhenAll(Enumerable.Range(1, howMany)
      .Select(async i =>
      {
        var driver = await _driverFixture.ADriver();
        var transit = await TransitDetails(driver, 20, SystemClock.Instance.InBclSystemDefaultZone().GetCurrentLocalDateTime(), client);
        await CreateAndResolveClaim(client, transit);
      }));
  }

  public async Task<Client> AClientWithClaims(Client.Types type, int howManyClaims)
  {
    var client = await _clientFixture.AClient(type);
    await ClientHasDoneClaimsAfterCompletedTransit(client, howManyClaims);
    return client;
  }

  public async Task ActiveAwardsAccount(Client client)
  {
    await _awardsAccountFixture.ActiveAwardsAccount(client);
  }
}