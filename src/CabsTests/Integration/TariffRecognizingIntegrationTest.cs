using LegacyFighter.Cabs.Controllers;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.CabsTests.Common;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

public class TariffRecognizingIntegrationTest
{
  private Fixtures Fixtures => _app.Fixtures;
  private TransitController TransitController => _app.TransitController;
  private CabsApp _app = default!;
  private IClock Clock { get; set; } = default!;

  [SetUp]
  public void InitializeApp()
  {
    Clock = Substitute.For<IClock>();
    _app = CabsApp.CreateInstance(services => services.AddSingleton(Clock));
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task NewYearsEveTariffShouldBeDisplayed()
  {
    //given
    var transitDto = await CreateTransit(new LocalDateTime(2021, 12, 31, 8, 30).InUtc().ToInstant());

    //when
    transitDto = await TransitController.GetTransit(transitDto.Id);

    //then
    Assert.AreEqual("Sylwester", transitDto.Tariff);
    Assert.AreEqual(3.5f, transitDto.KmRate);

  }

  [Test]
  public async Task WeekendTariffShouldBeDisplayed()
  {
    //given
    var transitDto = await CreateTransit(new LocalDateTime(2021, 4, 17, 8, 30).InUtc().ToInstant());

    //when
    transitDto = await TransitController.GetTransit(transitDto.Id);

    //then
    Assert.AreEqual("Weekend", transitDto.Tariff);
    Assert.AreEqual(1.5f, transitDto.KmRate);
  }

  [Test]
  public async Task WeekendPlusTariffShouldBeDisplayed()
  {
    //given
    var transitDto = await CreateTransit(new LocalDateTime(2021, 4, 17, 22, 30).InUtc().ToInstant());

    //when
    transitDto = await TransitController.GetTransit(transitDto.Id);

    //then
    Assert.AreEqual("Weekend+", transitDto.Tariff);
    Assert.AreEqual(2.5f, transitDto.KmRate);
  }

  [Test]
  public async Task StandardTariffShouldBeDisplayed()
  {
    //given
    var transitDto = await CreateTransit(new LocalDateTime(2021, 4, 13, 22, 30).InUtc().ToInstant());

    //when
    transitDto = await TransitController.GetTransit(transitDto.Id);

    //then
    Assert.AreEqual("Standard", transitDto.Tariff);
    Assert.AreEqual(1.0f, transitDto.KmRate);
  }

  private async Task<TransitDto> CreateTransit(Instant when) 
  {
    var client = await Fixtures.AClient();
    Clock.GetCurrentInstant().Returns(when);
    var transitDto = new TransitDto();
    var destination = new AddressDto("Polska", "Warszawa", "Zytnia", 20);
    var from = new AddressDto("Polska", "Warszawa", "Młynarska", 20);
    transitDto.From = from;
    transitDto.To = destination;
    var clientDto = new ClientDto();
    clientDto.Id = client.Id;
    transitDto.ClientDto = clientDto;
    return await TransitController.CreateTransit(transitDto);
  }
}