using System.Collections.Generic;
using LegacyFighter.Cabs.TransitAnalyzer;
using LegacyFighter.CabsTests.Common;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

public class GraphTransitAnalyzerIntegrationTest : TestWithGraphDb
{
  private CabsApp _app = default!;
  private GraphTransitAnalyzer Analyzer => _app.GraphTransitAnalyzer;

  [SetUp]
  public void InitializeApp()
  {
    _app = CabsApp.CreateInstance(new Dictionary<string, string>
    {
      ["GraphDatabase:Uri"] = Neo4JBoltUri
    });
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task CanRecognizeNewAddress()
  {
    //given
    await Analyzer.AddTransitBetweenAddresses(1L, 1L, 111, 222, 
      SystemClock.Instance.GetCurrentInstant(),
      SystemClock.Instance.GetCurrentInstant());
    await Analyzer.AddTransitBetweenAddresses(1L, 1L, 222, 333, 
      SystemClock.Instance.GetCurrentInstant(),
      SystemClock.Instance.GetCurrentInstant());
    await Analyzer.AddTransitBetweenAddresses(1L, 1L, 333, 444, 
      SystemClock.Instance.GetCurrentInstant(),
      SystemClock.Instance.GetCurrentInstant());

    //when
    var result = await Analyzer.Analyze(1L, 111);

    //then
    result.Should().Equal(111L, 222L, 333L, 444L);
  }
}