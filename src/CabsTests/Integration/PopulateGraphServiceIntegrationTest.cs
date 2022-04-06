using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Crm.TransitAnalyzer;
using LegacyFighter.Cabs.DriverFleet.DriverReports;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.CabsTests.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LegacyFighter.CabsTests.Integration;

internal class PopulateGraphServiceIntegrationTest : TestWithGraphDb
{
  private CabsApp _app = default!;
  private Fixtures Fixtures => _app.Fixtures;
  private IPopulateGraphService PopulateGraphService => _app.PopulateGraphService;
  private GraphTransitAnalyzer Analyzer => _app.GraphTransitAnalyzer;
  private IGeocodingService GeocodingService { get; set; } = default!;

  [SetUp]
  public void InitializeApp()
  {
    GeocodingService = Substitute.For<IGeocodingService>();
    _app = CabsApp.CreateInstance(
      collection => collection.AddSingleton(GeocodingService),
      ConfigurationOverridingGraphDatabaseUri());
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task CanPopulateGraphWithDataFromRelationalDb()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    var address1 = new Address("100_1", "1", "1", "1", 1);
    var address2 = new Address("100_2", "2", "2", "2", 2);
    var address3 = new Address("100_3", "3", "3", "3", 3);
    var address4 = new Address("100_4", "4", "4", "4", 3);
    //and
    _app.StartReuseRequestScope();
    await ATransitFromTo(address1, address2, client);
    await ATransitFromTo(address2, address3, client);
    await ATransitFromTo(address3, address4, client);
    _app.EndReuseRequestScope();

    //when
    await PopulateGraphService.Populate();

    //then
    var result = await Analyzer.Analyze(client.Id, address1.Hash);
    result.Should().Equal(
      address1.Hash.ToLong(),
      address2.Hash.ToLong(),
      address3.Hash.ToLong(),
      address4.Hash.ToLong());
  }

  private async Task ATransitFromTo(Address pickup, Address destination, Client client) 
  {
    GeocodingService.GeocodeAddress(destination).Returns(new double[]{1, 1});
    var driver = await Fixtures.ANearbyDriver(GeocodingService, pickup);
    await Fixtures.AJourney(50, client, driver, pickup, destination);
  }
}