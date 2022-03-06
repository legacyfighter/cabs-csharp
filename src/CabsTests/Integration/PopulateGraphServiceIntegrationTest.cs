using LegacyFighter.Cabs.DriverReports;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.TransitAnalyzer;
using LegacyFighter.CabsTests.Common;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

internal class PopulateGraphServiceIntegrationTest : TestWithGraphDb
{
  private CabsApp _app = default!;
  private Fixtures Fixtures => _app.Fixtures;
  private ITransitRepository TransitRepository => _app.TransitRepository;
  private IPopulateGraphService PopulateGraphService => _app.PopulateGraphService;
  private GraphTransitAnalyzer Analyzer => _app.GraphTransitAnalyzer;

  [SetUp]
  public void InitializeApp()
  {
    _app = CabsApp.CreateInstance(ConfigurationOverridingGraphDatabaseUri());
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
    var driver = await Fixtures.ADriver();
    //and
    var address1 = new Address("100_1", "1", "1", "1", 1);
    var address2 = new Address("100_2", "2", "2", "2", 2);
    var address3 = new Address("100_3", "3", "3", "3", 3);
    var address4 = new Address("100_4", "4", "4", "4", 3);
    //and
    _app.StartReuseRequestScope();
    await Fixtures.ARequestedAndCompletedTransit(10, Now(), Now(), client, driver, address1, address2);
    await Fixtures.ARequestedAndCompletedTransit(10, Now(), Now(), client, driver, address2, address3);
    await Fixtures.ARequestedAndCompletedTransit(10, Now(), Now(), client, driver, address3, address4);
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

  private static Instant Now()
  {
    return SystemClock.Instance.GetCurrentInstant();
  }
}