using System;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.CabsTests.Common;

namespace LegacyFighter.CabsTests.Integration;

public class ValidateDriverLicenseIntegrationTest
{
  private IDriverService DriverService => _app.DriverService;
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
  public async Task CannotCreateActiveDriverWithInvalidLicense()
  {
    //expect
    await this.Awaiting(_ => CreateActiveDriverWithLicense("invalidLicense"))
      .Should().ThrowExactlyAsync<ArgumentException>();
  }

  [Test]
  public async Task CanCreateActiveDriverWithValidLicense()
  {
    //when
    var driver = await CreateActiveDriverWithLicense("FARME100165AB5EW");

    //then
    var loaded = await Load(driver);
    Assert.AreEqual("FARME100165AB5EW", loaded.DriverLicense);
    Assert.AreEqual(Driver.Statuses.Active, loaded.Status);
  }

  [Test]
  public async Task CanCreateInactiveDriverWithInvalidLicense()
  {
    //when
    var driver = await CreateInactiveDriverWithLicense("invalidlicense");

    //then
    var loaded = await Load(driver);
    Assert.AreEqual("invalidlicense", loaded.DriverLicense);
    Assert.AreEqual(Driver.Statuses.Inactive, loaded.Status);
  }

  [Test]
  public async Task CanChangeLicenseForValidOne()
  {
    //given
    var driver = await CreateActiveDriverWithLicense("FARME100165AB5EW");

    //when
    await ChangeLicenseTo("99999740614992TL", driver);

    //then
    var loaded = await Load(driver);
    Assert.AreEqual("99999740614992TL", loaded.DriverLicense);
  }

  [Test]
  public async Task CannotChangeLicenseForInvalidOne()
  {
    //given
    var driver = await CreateActiveDriverWithLicense("FARME100165AB5EW");

    //expect
    await this.Awaiting(_ => ChangeLicenseTo("invalid", driver))
      .Should().ThrowExactlyAsync<ArgumentException>();
  }

  [Test]
  public async Task CanActivateDriverWithValidLicense()
  {
    //given
    var driver = await CreateInactiveDriverWithLicense("FARME100165AB5EW");

    //when
    await Activate(driver);

    //then
    var loaded = await Load(driver);
    Assert.AreEqual(Driver.Statuses.Active, loaded.Status);
  }

  [Test]
  public async Task CannotActivateDriverWithInvalidLicense()
  {
    //given
    var driver = await CreateInactiveDriverWithLicense("invalid");

    //exoect
    await this.Awaiting(_ => Activate(driver))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  private async Task<Driver> CreateActiveDriverWithLicense(string license)
  {
    return await DriverService.CreateDriver(
      license,
      "Kowalski",
      "Jan",
      Driver.Types.Regular,
      Driver.Statuses.Active,
      "photo");
  }

  private async Task<Driver> CreateInactiveDriverWithLicense(string license)
  {
    return await DriverService.CreateDriver(license, "Kowalski", "Jan", Driver.Types.Regular, Driver.Statuses.Inactive,
      "photo");
  }

  private async Task<DriverDto> Load(Driver driver)
  {
    var loaded = await DriverService.LoadDriver(driver.Id);
    return loaded;
  }

  private async Task ChangeLicenseTo(string newLicense, Driver driver)
  {
    await DriverService.ChangeLicenseNumber(newLicense, driver.Id);
  }

  private async Task Activate(Driver driver)
  {
    await DriverService.ChangeDriverStatus(driver.Id, Driver.Statuses.Active);
  }
}