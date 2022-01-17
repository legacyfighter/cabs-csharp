using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using LegacyFighter.CabsTests.Common;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using System.Collections.Generic;
using System.Linq;

namespace LegacyFighter.CabsTests.Integration;

public class RemovingAwardMilesIntegrationTest
{
  private static readonly Instant DayBeforeYesterday = new LocalDateTime(1989, 12, 12, 12, 12).InUtc().ToInstant();
  private static readonly Instant Yesterday = DayBeforeYesterday.Plus(Duration.FromDays(1));
  private static readonly Instant Today = Yesterday.Plus(Duration.FromDays(1));
  private static readonly Instant Sunday = new LocalDateTime(1989, 12, 17, 12, 12).InUtc().ToInstant();

  private CabsApp _app = default!;
  private Fixtures Fixtures => _app.Fixtures;
  private IAwardsService AwardsService => _app.AwardsService;
  private IAwardedMilesRepository AwardedMilesRepository => _app.AwardedMilesRepository;
  private IClock Clock { get; set; } = default!;
  private IAppProperties AppProperties { get; set; } = default!;

  [SetUp]
  public void InitializeApp()
  {
    AppProperties = Substitute.For<IAppProperties>();
    Clock = Substitute.For<IClock>();
    _app = CabsApp.CreateInstance(collection =>
    {
      collection.AddSingleton(Clock);
      collection.AddSingleton(AppProperties);
    });
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task ByDefaultRemoveOldestFirstEvenWhenTheyAreNonExpiring()
  {
    //given
    var client = await ClientWithAnActiveMilesProgram(Client.Types.Normal);
    //and
    var transit = await Fixtures.ATransit(new Money(80));
    //and
    var middle = await GrantedMilesThatWillExpireInDays(10, 365, Yesterday, client, transit);
    var youngest = await GrantedMilesThatWillExpireInDays(10, 365, Today, client, transit);
    var oldestNonExpiringMiles = await GrantedNonExpiringMiles(5, DayBeforeYesterday, client);

    //when
    await AwardsService.RemoveMiles(client.Id, 16);

    //then
    var awardedMiles = await AwardedMilesRepository.FindAllByClient(client);
    AssertThatMilesWereReducedTo(oldestNonExpiringMiles, 0, awardedMiles);
    AssertThatMilesWereReducedTo(middle, 0, awardedMiles);
    AssertThatMilesWereReducedTo(youngest, 9, awardedMiles);
  }

  [Test]
  public async Task ShouldRemoveOldestMilesFirstWhenManyTransits()
  {
    //given
    var client = await ClientWithAnActiveMilesProgram(Client.Types.Normal);
    //and
    await Fixtures.ClientHasDoneTransits(client, 15);
    //and
    var transit = await Fixtures.ATransit(new Money(80));
    //and
    var oldest = await GrantedMilesThatWillExpireInDays(10, 60, DayBeforeYesterday, client, transit);
    var middle = await GrantedMilesThatWillExpireInDays(10, 365, Yesterday, client, transit);
    var youngest = await GrantedMilesThatWillExpireInDays(10, 30, Today, client, transit);

    //when
    await AwardsService.RemoveMiles(client.Id, 15);

    //then
    var awardedMiles = await AwardedMilesRepository.FindAllByClient(client);
    AssertThatMilesWereReducedTo(oldest, 0, awardedMiles);
    AssertThatMilesWereReducedTo(middle, 5, awardedMiles);
    AssertThatMilesWereReducedTo(youngest, 10, awardedMiles);
  }

  [Test]
  public async Task ShouldRemoveNonExpiringMilesLastWhenManyTransits()
  {
    //given
    var client = await ClientWithAnActiveMilesProgram(Client.Types.Normal);
    //and
    await Fixtures.ClientHasDoneTransits(client, 15);
    //and
    var transit = await Fixtures.ATransit(new Money(80));

    var regularMiles = await GrantedMilesThatWillExpireInDays(10, 365, Today, client, transit);
    var oldestNonExpiringMiles = await GrantedNonExpiringMiles(5, DayBeforeYesterday, client);

    //when
    await AwardsService.RemoveMiles(client.Id, 13);

    //then
    var awardedMiles = await AwardedMilesRepository.FindAllByClient(client);
    AssertThatMilesWereReducedTo(regularMiles, 0, awardedMiles);
    AssertThatMilesWereReducedTo(oldestNonExpiringMiles, 2, awardedMiles);
  }

  [Test]
  public async Task ShouldRemoveSoonToExpireMilesFirstWhenClientIsVip()
  {
    //given
    var client = await ClientWithAnActiveMilesProgram(Client.Types.Vip);
    //and
    var transit = await Fixtures.ATransit(new Money(80));
    //and
    var secondToExpire = await GrantedMilesThatWillExpireInDays(10, 60, Yesterday, client, transit);
    var thirdToExpire = await GrantedMilesThatWillExpireInDays(5, 365, DayBeforeYesterday, client, transit);
    var firstToExpire = await GrantedMilesThatWillExpireInDays(15, 30, Today, client, transit);
    var nonExpiringMiles = await GrantedNonExpiringMiles(1, DayBeforeYesterday, client);

    //when
    await AwardsService.RemoveMiles(client.Id, 21);

    //then
    var awardedMiles = await AwardedMilesRepository.FindAllByClient(client);
    AssertThatMilesWereReducedTo(nonExpiringMiles, 1, awardedMiles);
    AssertThatMilesWereReducedTo(firstToExpire, 0, awardedMiles);
    AssertThatMilesWereReducedTo(secondToExpire, 4, awardedMiles);
    AssertThatMilesWereReducedTo(thirdToExpire, 5, awardedMiles);
  }

  [Test]
  public async Task shouldRemoveSoonToExpireMilesFirstWhenRemovingOnSundayAndClientHasDoneManyTransits()
  {
    //given
    var client = await ClientWithAnActiveMilesProgram(Client.Types.Normal);
    //and
    await Fixtures.ClientHasDoneTransits(client, 15);
    //and
    var transit = await Fixtures.ATransit(new Money(80));
    //and
    var secondToExpire = await GrantedMilesThatWillExpireInDays(10, 60, Yesterday, client, transit);
    var thirdToExpire = await GrantedMilesThatWillExpireInDays(5, 365, DayBeforeYesterday, client, transit);
    var firstToExpire = await GrantedMilesThatWillExpireInDays(15, 10, Today, client, transit);
    var nonExpiringMiles = await GrantedNonExpiringMiles(100, Yesterday, client);

    //when
    ItIsSunday();
    await AwardsService.RemoveMiles(client.Id, 21);

    //then
    var awardedMiles = await AwardedMilesRepository.FindAllByClient(client);
    AssertThatMilesWereReducedTo(nonExpiringMiles, 100, awardedMiles);
    AssertThatMilesWereReducedTo(firstToExpire, 0, awardedMiles);
    AssertThatMilesWereReducedTo(secondToExpire, 4, awardedMiles);
    AssertThatMilesWereReducedTo(thirdToExpire, 5, awardedMiles);
  }

  [Test]
  public async Task ShouldRemoveExpiringMilesFirstWhenClientHasManyClaims()
  {
    //given
    var client = await ClientWithAnActiveMilesProgram(Client.Types.Normal);
    //and
    await Fixtures.ClientHasDoneClaims(client, 3);
    //and
    var transit = await Fixtures.ATransit(new Money(80));
    //and
    var secondToExpire = await GrantedMilesThatWillExpireInDays(4, 60, Yesterday, client, transit);
    var thirdToExpire = await GrantedMilesThatWillExpireInDays(10, 365, DayBeforeYesterday, client, transit);
    var firstToExpire = await GrantedMilesThatWillExpireInDays(5, 10, Yesterday, client, transit);
    var nonExpiringMiles = await GrantedNonExpiringMiles(10, Yesterday, client);

    //when
    await AwardsService.RemoveMiles(client.Id, 21);

    //then
    var awardedMiles = await AwardedMilesRepository.FindAllByClient(client);
    AssertThatMilesWereReducedTo(nonExpiringMiles, 0, awardedMiles);
    AssertThatMilesWereReducedTo(thirdToExpire, 0, awardedMiles);
    AssertThatMilesWereReducedTo(secondToExpire, 3, awardedMiles);
    AssertThatMilesWereReducedTo(firstToExpire, 5, awardedMiles);
  }

  private async Task<AwardedMiles> GrantedMilesThatWillExpireInDays(int miles, int expirationInDays, Instant when,
    Client client, Transit transit)
  {
    MilesWillExpireInDays(expirationInDays);
    DefaultMilesBonusIs(miles);
    return await MilesRegisteredAt(when, client, transit);
  }

  private async Task<AwardedMiles> GrantedNonExpiringMiles(int miles, Instant when, Client client)
  {
    DefaultMilesBonusIs(miles);
    Clock.GetCurrentInstant().Returns(when);
    return await AwardsService.RegisterNonExpiringMiles(client.Id, miles);
  }

  private void AssertThatMilesWereReducedTo(AwardedMiles firstToExpire, int milesAfterReduction,
    List<AwardedMiles> allMiles)
  {
    var actual = allMiles
      .Where(am => firstToExpire.Id == am.Id).Select(am => am.Miles);
    actual.First().Should().Be(milesAfterReduction);
  }

  private async Task<AwardedMiles> MilesRegisteredAt(Instant when, Client client, Transit transit)
  {
    Clock.GetCurrentInstant().Returns(when);
    return await AwardsService.RegisterMiles(client.Id, transit.Id);
  }

  private async Task<Client> ClientWithAnActiveMilesProgram(Client.Types type)
  {
    Clock.GetCurrentInstant().Returns(DayBeforeYesterday);
    var client = await Fixtures.AClient(type);
    await Fixtures.ActiveAwardsAccount(client);
    return client;
  }

  private void MilesWillExpireInDays(int days)
  {
    AppProperties.MilesExpirationInDays.Returns(days);
  }

  private void DefaultMilesBonusIs(int miles)
  {
    AppProperties.DefaultMilesBonus.Returns(miles);
  }

  private void ItIsSunday()
  {
    Clock.GetCurrentInstant().Returns(Sunday);
  }
}