﻿using System;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using LegacyFighter.CabsTests.Common;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

public class AwardMilesManagementIntegrationTest
{
  private static readonly Instant Now = new LocalDateTime(1989, 12, 12, 12, 12).InUtc().ToInstant();
  private CabsApp _app = default!;
  private Fixtures Fixtures => _app.Fixtures;
  private IAwardsService AwardsService => _app.AwardsService;
  private IAwardedMilesRepository AwardedMilesRepository => _app.AwardedMilesRepository;
  private IClock Clock { get; set; } = default!;

  [SetUp]
  public void InitializeApp()
  {
    Clock = Substitute.For<IClock>();
    Clock.GetCurrentInstant().Returns(Now);
    _app = CabsApp.CreateInstance(collection =>
      collection.AddSingleton(Clock));
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task CanRegisterAccount()
  {
    //given
    var client = await Fixtures.AClient();

    //when
    await AwardsService.RegisterToProgram(client.Id);

    //then
    var account = await AwardsService.FindBy(client.Id);
    Assert.NotNull(account);
    Assert.AreEqual(client.Id, account.Client.Id);
    Assert.False(account.Active);
    Assert.AreEqual(0, account.Transactions);
  }

  [Test]
  public async Task CanActivateAccount()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    await AwardsService.RegisterToProgram(client.Id);

    //when
    await AwardsService.ActivateAccount(client.Id);

    //then
    var account = await AwardsService.FindBy(client.Id);
    Assert.True(account.Active);
  }

  [Test]
  public async Task CanDeactivateAccount()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    await Fixtures.ActiveAwardsAccount(client);

    //when
    await AwardsService.DeactivateAccount(client.Id);

    //then
    var account = await AwardsService.FindBy(client.Id);
    Assert.False(account.Active);
  }

  [Test]
  public async Task CanRegisterMiles()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    await Fixtures.ActiveAwardsAccount(client);
    //and
    var transit = await Fixtures.ATransit(new Money(80));

    //when
    await AwardsService.RegisterMiles(client.Id, transit.Id);

    //then
    var account = await AwardsService.FindBy(client.Id);
    Assert.AreEqual(1, account.Transactions);
    var awardedMiles = await AwardedMilesRepository.FindAllByClient(client);
    Assert.AreEqual(1, awardedMiles.Count);
    Assert.AreEqual(10, awardedMiles[0].Miles);
    Assert.False(awardedMiles[0].CantExpire);

  }

  [Test]
  public async Task CanRegisterNonExpiringMiles()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    await Fixtures.ActiveAwardsAccount(client);

    //when
    await AwardsService.RegisterNonExpiringMiles(client.Id, 20);

    //then
    var account = await AwardsService.FindBy(client.Id);
    Assert.AreEqual(1, account.Transactions);
    var awardedMiles = await AwardedMilesRepository.FindAllByClient(client);
    Assert.AreEqual(1, awardedMiles.Count);
    Assert.AreEqual(20, awardedMiles[0].Miles);
    Assert.True(awardedMiles[0].CantExpire);
  }

  [Test]
  public async Task CanCalculateMilesBalance()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    await Fixtures.ActiveAwardsAccount(client);
    //and
    var transit = await Fixtures.ATransit(new Money(80));

    //when
    await AwardsService.RegisterNonExpiringMiles(client.Id, 20);
    await AwardsService.RegisterMiles(client.Id, transit.Id);
    await AwardsService.RegisterMiles(client.Id, transit.Id);

    //then
    var account = await AwardsService.FindBy(client.Id);
    Assert.AreEqual(3, account.Transactions);
    var miles = await AwardsService.CalculateBalance(client.Id);
    Assert.AreEqual(40, miles);
  }

  [Test]
  public async Task CanTransferMiles()
  {
    //given
    var client = await Fixtures.AClient();
    var secondClient = await Fixtures.AClient();
    //and
    await Fixtures.ActiveAwardsAccount(client);
    await Fixtures.ActiveAwardsAccount(secondClient);
    //and
    await AwardsService.RegisterNonExpiringMiles(client.Id, 10);

    //when
    await AwardsService.TransferMiles(client.Id, secondClient.Id, 10);

    //then
    var firstClientBalance = await AwardsService.CalculateBalance(client.Id);
    var secondClientBalance = await AwardsService.CalculateBalance(secondClient.Id);
    Assert.AreEqual(0, firstClientBalance);
    Assert.AreEqual(10, secondClientBalance);
  }

  [Test]
  public async Task CannotTransferMilesWhenAccountIsNotActive()
  {
    //given
    var client = await Fixtures.AClient();
    var secondClient = await Fixtures.AClient();
    //and
    await Fixtures.ActiveAwardsAccount(client);
    await Fixtures.ActiveAwardsAccount(secondClient);
    //and
    await AwardsService.RegisterNonExpiringMiles(client.Id, 10);
    //and
    await AwardsService.DeactivateAccount(client.Id);

    //when
    await AwardsService.TransferMiles(client.Id, secondClient.Id, 5);

    //then
    Assert.AreEqual(10, await AwardsService.CalculateBalance(client.Id));
  }

  [Test]
  public async Task CannotTransferMilesWhenNotEnough()
  {
    //given
    var client = await Fixtures.AClient();
    var secondClient = await Fixtures.AClient();
    //and
    await Fixtures.ActiveAwardsAccount(client);
    await Fixtures.ActiveAwardsAccount(secondClient);
    //and
    await AwardsService.RegisterNonExpiringMiles(client.Id, 10);

    //when
    await AwardsService.TransferMiles(client.Id, secondClient.Id, 30);

    //then
    Assert.AreEqual(10, await AwardsService.CalculateBalance(client.Id));
  }

  [Test]
  public async Task CannotTransferMilesWhenAccountNotActive()
  {
    //given
    var client = await Fixtures.AClient();
    var secondClient = await Fixtures.AClient();
    //and
    await Fixtures.ActiveAwardsAccount(client);
    await Fixtures.ActiveAwardsAccount(secondClient);
    //and
    await AwardsService.RegisterNonExpiringMiles(client.Id, 10);
    //and
    await AwardsService.DeactivateAccount(client.Id);

    //when
    await AwardsService.TransferMiles(client.Id, secondClient.Id, 5);
    //then
    Assert.AreEqual(10, await AwardsService.CalculateBalance(client.Id));
  }

  [Test]
  public async Task CanRemoveMiles()
  {
    //given
    var client = await Fixtures.AClient(Client.Types.Normal);
    //and
    await Fixtures.ActiveAwardsAccount(client);
    //and
    var transit = await Fixtures.ATransit(new Money(80));
    //and
    await AwardsService.RegisterMiles(client.Id, transit.Id);
    await AwardsService.RegisterMiles(client.Id, transit.Id);
    await AwardsService.RegisterMiles(client.Id, transit.Id);

    //when
    await AwardsService.RemoveMiles(client.Id, 20);

    //then
    var miles = await AwardsService.CalculateBalance(client.Id);
    Assert.AreEqual(10, miles);
  }

  [Test]
  public async Task CannotRemoveMoreThanClientHasMiles()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    await Fixtures.ActiveAwardsAccount(client);
    //and
    var transit = await Fixtures.ATransit(new Money(80));

    //when
    await AwardsService.RegisterMiles(client.Id, transit.Id);
    await AwardsService.RegisterMiles(client.Id, transit.Id);
    await AwardsService.RegisterMiles(client.Id, transit.Id);

    //then
    await AwardsService.Awaiting(s => s.RemoveMiles(client.Id, 40))
      .Should().ThrowAsync<ArgumentException>();
  }

  [Test]
  public async Task CannotRemoveMilesIfAccountIsNotActive()
  {
    //given
    var client = await Fixtures.AClient();
    //and
    await AwardsService.RegisterToProgram(client.Id);
    //and
    var transit = await Fixtures.ATransit(new Money(80));
    //and
    var currentMiles = await AwardsService.CalculateBalance(client.Id);

    //when
    await AwardsService.RegisterMiles(client.Id, transit.Id);

    //then
    Assert.AreEqual(currentMiles, await AwardsService.CalculateBalance(client.Id));
  }
}