using System;
using System.Linq;
using FluentAssertions.Extensions;
using LegacyFighter.Cabs.Controllers;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Service;
using LegacyFighter.CabsTests.Common;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

public class AnalyzeNearbyTransitsIntegrationTest : TestWithGraphDb
{
  private CabsApp _app = default!;
  private Fixtures Fixtures => _app.Fixtures;
  private TransitAnalyzerController TransitAnalyzerController => _app.TransitAnalyzerController;
  private IGeocodingService GeocodingService { get; set; } = default!;
  private IClock Clock { get; set; } = default!;

  [SetUp]
  public async Task InitializeApp()
  {
    Clock = Substitute.For<IClock>();
    GeocodingService = Substitute.For<IGeocodingService>();
    _app = CabsApp.CreateInstance(collection =>
    {
      collection.AddSingleton(Clock);
      collection.AddSingleton(GeocodingService);
    }, ConfigurationOverridingGraphDatabaseUri());
    await Fixtures.AnActiveCarCategory(CarType.CarClasses.Van);
    GeocodingService.GeocodeAddress(Arg.Any<Address>()).Returns(new double[] { 1, 1 });
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task CanFindLongestTravel()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    Clock.GetCurrentInstant().Returns(Instant.FromUtc(2021, 1, 1, 0, 00));
    var driver = await Fixtures.ANearbyDriver("WA001");
    //and
    var address1 = new Address("1_1", "1", "1", "1", 1);
    var address2 = new Address("1_2", "2", "2", "2", 2);
    var address3 = new Address("1_3", "3", "3", "3", 3);
    var address4 = new Address("1_4", "4", "4", "4", 3);
    var address5 = new Address("1_5", "5", "5", "5", 3);
    //and
    // 1-2-3-4
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 00),
      Instant.FromUtc(2021, 1, 1, 0, 10), client, driver, address1, address2, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 15),
      Instant.FromUtc(2021, 1, 1, 0, 20), client, driver, address2, address3, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 25),
      Instant.FromUtc(2021, 1, 1, 0, 30), client, driver, address3, address4, Clock);
    // 1-2-3
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 2, 0, 00),
      Instant.FromUtc(2021, 1, 2, 0, 10), client, driver, address1, address2, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 2, 0, 15),
      Instant.FromUtc(2021, 1, 2, 0, 20), client, driver, address2, address3, Clock);
    // 1-3
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 3, 0, 00),
      Instant.FromUtc(2021, 1, 3, 0, 10), client, driver, address1, address3, Clock);
    // 3-1-2-5-4-5
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 2, 1, 0, 00),
      Instant.FromUtc(2021, 2, 1, 0, 10), client, driver, address3, address1, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 2, 1, 0, 20),
      Instant.FromUtc(2021, 2, 1, 0, 25), client, driver, address1, address2, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 2, 1, 0, 30),
      Instant.FromUtc(2021, 2, 1, 0, 35), client, driver, address2, address5, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 2, 1, 0, 40),
      Instant.FromUtc(2021, 2, 1, 0, 45), client, driver, address5, address4, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 2, 1, 0, 50),
      Instant.FromUtc(2021, 2, 1, 0, 55), client, driver, address4, address5, Clock);

    await AddressesContainExactly(
      //when
      async() => await TransitAnalyzerController.Analyze(client.Id, address1.Id), 
      //then
      // 1-2-5-4-5
      address1, address2, address5, address4, address5);
  }

  [Test]
  public async Task CanFindLongestTravelFromMultipleClients()
  {
    //given
    var client1 = await Fixtures.AClient();
    var client2 = await Fixtures.AClient();
    var client3 = await Fixtures.AClient();
    var client4 = await Fixtures.AClient();
    //and
    Clock.GetCurrentInstant().Returns(Instant.FromUtc(2021, 1, 1, 0, 00));
    var driver = await Fixtures.ANearbyDriver("WA001");
    //and
    var address1 = new Address("2_1", "1", "1", "1", 1);
    var address2 = new Address("2_2", "2", "2", "2", 2);
    var address3 = new Address("2_3", "3", "3", "3", 3);
    var address4 = new Address("2_4", "4", "4", "4", 3);
    var address5 = new Address("2_5", "5", "5", "5", 3);
    //and
    // 1-2-3-4
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 00),
      Instant.FromUtc(2021, 1, 1, 0, 10), client1, driver, address1, address2, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 15),
      Instant.FromUtc(2021, 1, 1, 0, 20), client1, driver, address2, address3, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 25),
      Instant.FromUtc(2021, 1, 1, 0, 30), client1, driver, address3, address4, Clock);
    // 1-2-3
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 2, 0, 00),
      Instant.FromUtc(2021, 1, 2, 0, 10), client2, driver, address1, address2, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 2, 0, 15),
      Instant.FromUtc(2021, 1, 2, 0, 20), client2, driver, address2, address3, Clock);
    // 1-3
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 3, 0, 00),
      Instant.FromUtc(2021, 1, 3, 0, 10), client3, driver, address1, address3, Clock);
    // 1-3-2-5-4-5
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 2, 1, 0, 00),
      Instant.FromUtc(2021, 2, 1, 0, 10), client4, driver, address1, address3, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 2, 1, 0, 20),
      Instant.FromUtc(2021, 2, 1, 0, 25), client4, driver, address3, address2, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 2, 1, 0, 30),
      Instant.FromUtc(2021, 2, 1, 0, 35), client4, driver, address2, address5, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 2, 1, 0, 40),
      Instant.FromUtc(2021, 2, 1, 0, 45), client4, driver, address5, address4, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 2, 1, 0, 50),
      Instant.FromUtc(2021, 2, 1, 0, 55), client4, driver, address4, address5, Clock);

    await AddressesContainExactly(
      //when
      async() => await TransitAnalyzerController.Analyze(client1.Id, address1.Id), 
      //then
      // 1-2-3-4
      address1, address2, address3, address4);
  }

  [Test]
  public async Task CanFindLongestTravelWithLongStops()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    Clock.GetCurrentInstant().Returns(Instant.FromUtc(2021, 1, 1, 0, 00));
    var driver = await Fixtures.ANearbyDriver("WA001");
    //and
    var address1 = new Address("3_1", "1", "1", "1", 1);
    var address2 = new Address("3_2", "2", "2", "2", 2);
    var address3 = new Address("3_3", "3", "3", "3", 3);
    var address4 = new Address("3_4", "4", "4", "4", 3);
    var address5 = new Address("3_5", "5", "5", "5", 3);
    //and
    // 1-2-3-4-(stop)-5-1
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 00),
      Instant.FromUtc(2021, 1, 1, 0, 05), client, driver, address1, address2, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 10),
      Instant.FromUtc(2021, 1, 1, 0, 15), client, driver, address2, address3, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 20),
      Instant.FromUtc(2021, 1, 1, 0, 25), client, driver, address3, address4, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 1, 00),
      Instant.FromUtc(2021, 1, 1, 1, 10), client, driver, address4, address5, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 1, 10),
      Instant.FromUtc(2021, 1, 1, 1, 15), client, driver, address5, address1, Clock);

    await AddressesContainExactly(
      //when
      async() => await TransitAnalyzerController.Analyze(client.Id, address1.Id), 
      //then
      // 1-2-3-4
      address1, address2, address3, address4);
  }

  [Test]
  public async Task CanFindLongestTravelWithLoops()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    Clock.GetCurrentInstant().Returns(Instant.FromUtc(2021, 1, 1, 0, 00));
    var driver = await Fixtures.ANearbyDriver("WA001");
    //and
    var address1 = new Address("4_1", "1", "1", "1", 1);
    var address2 = new Address("4_2", "2", "2", "2", 2);
    var address3 = new Address("4_3", "3", "3", "3", 3);
    var address4 = new Address("4_4", "4", "4", "4", 3);
    var address5 = new Address("4_5", "5", "5", "5", 3);
    //and
    // 5-1-2-3
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 00),
      Instant.FromUtc(2021, 1, 1, 0, 5), client, driver, address5, address1, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 6),
      Instant.FromUtc(2021, 1, 1, 0, 10), client, driver, address1, address2, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 15),
      Instant.FromUtc(2021, 1, 1, 0, 20), client, driver, address2, address3, Clock);
    // 3-2-1
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 2, 0, 00),
      Instant.FromUtc(2021, 1, 2, 0, 10), client, driver, address3, address2, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 2, 0, 15),
      Instant.FromUtc(2021, 1, 2, 0, 20), client, driver, address2, address1, Clock);
    // 1-5
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 3, 0, 00),
      Instant.FromUtc(2021, 1, 3, 0, 10), client, driver, address1, address5, Clock);
    // 3-1-2-5-4-5
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2000, 2, 1, 0, 00),
      Instant.FromUtc(2020, 2, 1, 0, 10), client, driver, address3, address1, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2020, 2, 1, 0, 20),
      Instant.FromUtc(2020, 2, 1, 0, 25), client, driver, address1, address2, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2020, 2, 1, 0, 30),
      Instant.FromUtc(2020, 2, 1, 0, 35), client, driver, address2, address5, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2020, 2, 1, 0, 40),
      Instant.FromUtc(2020, 2, 1, 0, 45), client, driver, address5, address4, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2020, 2, 1, 0, 50),
      Instant.FromUtc(2020, 2, 1, 0, 55), client, driver, address4, address5, Clock);

    await AddressesContainExactly(
      //when
      async() => await TransitAnalyzerController.Analyze(client.Id, address5.Id), 
      //then
      // 5-1-2-3
      address5, address1, address2, address3);
  }

  [Test]
  public async Task CanFindLongTravelBetweenOthers()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    Clock.GetCurrentInstant().Returns(Instant.FromUtc(2021, 1, 1, 0, 00));
    var driver = await Fixtures.ANearbyDriver("WA001");
    //and
    var address1 = new Address("5_1", "1", "1", "1", 1);
    var address2 = new Address("5_2", "2", "2", "2", 2);
    var address3 = new Address("5_3", "3", "3", "3", 3);
    var address4 = new Address("5_4", "4", "4", "4", 3);
    var address5 = new Address("5_5", "4", "4", "4", 3);
    //and
    // 1-2-3
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 00),
      Instant.FromUtc(2021, 1, 1, 0, 5), client, driver, address1, address2, Clock);
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 10),
      Instant.FromUtc(2021, 1, 1, 0, 15), client, driver, address2, address3, Clock);
    // 4-5
    await Fixtures.ARequestedAndCompletedTransit(50, Instant.FromUtc(2021, 1, 1, 0, 20),
      Instant.FromUtc(2021, 1, 1, 0, 25), client, driver, address4, address5, Clock);

    await AddressesContainExactly(
      //when
      async() => await TransitAnalyzerController.Analyze(client.Id, address1.Id), 
      //then
      //1-2
      address1, address2, address3);
  }

  private async Task AddressesContainExactly(Func<Task<AnalyzedAddressesDto>> actualSource, params Address[] addresses)
  {
    await TransitAnalyzerController.Awaiting(new Func<TransitAnalyzerController, Task>(async _ =>
    {
      var analyzedAddressesDto = await actualSource();

      var expectedHashes = addresses.Select(a => a.Hash).ToList();
      analyzedAddressesDto.Addresses.Select(a => a.Hash).ToList().Should().Equal(expectedHashes);
    })).Should().NotThrowAfterAsync(10.Seconds(), 1.Seconds());
  }
}