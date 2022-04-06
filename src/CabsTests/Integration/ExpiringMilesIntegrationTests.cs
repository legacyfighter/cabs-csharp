using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Service;
using LegacyFighter.CabsTests.Common;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

public class ExpiringMilesIntegrationTest
{
  private const long TransitId = 1L;
  private static readonly Instant _1989_12_12 = new LocalDateTime(1989, 12, 12, 12, 12).InUtc().ToInstant();
  private static readonly Instant _1989_12_13 = _1989_12_12.Plus(Duration.FromDays(1));
  private static readonly Instant _1989_12_14 = _1989_12_13.Plus(Duration.FromDays(1));

  private CabsApp _app = default!;
  private Fixtures Fixtures => _app.Fixtures;
  private IAwardsService AwardsService => _app.AwardsService;
  private IClock Clock { get; set; } = default!;
  private IAppProperties AppProperties { get; set; } = default!;

  [SetUp]
  public void InitializeApp()
  {
    AppProperties = Substitute.For<IAppProperties>();
    Clock = Substitute.For<IClock>();
    _app = CabsApp.CreateInstance(collection => collection.AddSingleton(Clock));
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task ShouldTakeIntoAccountExpiredMilesWhenCalculatingBalance()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    Clock.GetCurrentInstant().Returns(_1989_12_12);
    //and
    DefaultMilesBonusIs(10);
    //and
    DefaultMilesExpirationInDaysIs(365);
    //and
    await Fixtures.ActiveAwardsAccount(client);

    //when
    await RegisterMilesAt(client, _1989_12_12);
    //then
    Assert.AreEqual(10, await CalculateBalanceAt(client, _1989_12_12));
    //when
    await RegisterMilesAt(client, _1989_12_13);
    //then
    Assert.AreEqual(20, await CalculateBalanceAt(client, _1989_12_12));
    //when
    await RegisterMilesAt(client, _1989_12_14);
    //then
    Assert.AreEqual(30, await CalculateBalanceAt(client, _1989_12_14));
    Assert.AreEqual(30, await CalculateBalanceAt(client, _1989_12_12.Plus(Duration.FromDays(300))));
    Assert.AreEqual(20, await CalculateBalanceAt(client, _1989_12_12.Plus(Duration.FromDays(365))));
    Assert.AreEqual(10, await CalculateBalanceAt(client, _1989_12_13.Plus(Duration.FromDays(365))));
    Assert.AreEqual(0, await CalculateBalanceAt(client, _1989_12_14.Plus(Duration.FromDays(365))));
  }

  private void DefaultMilesBonusIs(int bonus)
  {
    AppProperties.DefaultMilesBonus.Returns(bonus);
  }

  private void DefaultMilesExpirationInDaysIs(int days)
  {
    AppProperties.MilesExpirationInDays.Returns(days);
  }

  private async Task RegisterMilesAt(Client client, Instant when)
  {
    Clock.GetCurrentInstant().Returns(when);
    await AwardsService.RegisterMiles(client.Id, TransitId);
  }

  private async Task<int?> CalculateBalanceAt(Client client, Instant when)
  {
    Clock.GetCurrentInstant().Returns(when);
    return await AwardsService.CalculateBalance(client.Id);
  }
}