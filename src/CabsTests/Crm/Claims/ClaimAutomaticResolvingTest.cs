using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Crm.Claims;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Ride;
using Statuses = LegacyFighter.Cabs.Crm.Claims.Statuses;

namespace LegacyFighter.CabsTests.Crm.Claims;

public class ClaimAutomaticResolvingTest
{
  [Test]
  public void SecondClaimForTheSameTransitWillBeEscalated()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var transit = ATransit(1L);
    //and
    var claim = CreateClaim(transit, 39);
    //and
    resolver.Resolve(claim, Client.Types.Normal, 40, 15, 10);
    //and
    var claim2 = CreateClaim(transit, 39);

    //when
    var result = resolver.Resolve(claim2, Client.Types.Normal, 40, 15, 10);

    //then
    Assert.AreEqual(Statuses.Escalated, result.Decision);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskNoOne, result.WhoToAsk);
  }

  [Test]
  public void LowCostTransitsAreRefundedIfClientIsVip()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var transit = ATransit(1L);
    //and
    var claim = CreateClaim(transit, 39);

    //when
    var result = resolver.Resolve(claim, Client.Types.Vip, 40, 15, 10);

    //then
    Assert.AreEqual(Statuses.Refunded, result.Decision);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskNoOne, result.WhoToAsk);
  }

  [Test]
  public void HighCostTransitsAreEscalatedEvenWhenClientIsVip()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var claim = CreateClaim(ATransit(1L), 39);
    resolver.Resolve(claim, Client.Types.Vip, 40, 15, 10);
    var claim2 = CreateClaim(ATransit(2L), 39);
    resolver.Resolve(claim2, Client.Types.Vip, 40, 15, 10);
    var claim3 = CreateClaim(ATransit(3L), 39);
    resolver.Resolve(claim3, Client.Types.Vip, 40, 15, 10);
    //and
    var claim4 = CreateClaim(ATransit(4L), 41);

    //when
    var result = resolver.Resolve(claim4, Client.Types.Vip, 40, 15, 10);

    //then
    Assert.AreEqual(Statuses.Escalated, result.Decision);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskDriver, result.WhoToAsk);
  }

  [Test]
  public void FirstThreeClaimsAreRefunded()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var claim = CreateClaim(ATransit(1L), 39);
    var result1 = resolver.Resolve(claim, Client.Types.Normal, 40, 15, 10);
    var claim2 = CreateClaim(ATransit(2L), 39);
    var result2 = resolver.Resolve(claim2, Client.Types.Normal, 40, 15, 10);
    var claim3 = CreateClaim(ATransit(3L), 39);
    var result3 = resolver.Resolve(claim3, Client.Types.Normal, 40, 15, 10);

    //when
    var claim4 = CreateClaim(ATransit(4L), 39);
    var result4 = resolver.Resolve(claim4, Client.Types.Normal, 40, 4, 10);

    //then
    Assert.AreEqual(Statuses.Refunded, result1.Decision);
    Assert.AreEqual(Statuses.Refunded, result2.Decision);
    Assert.AreEqual(Statuses.Refunded, result3.Decision);
    Assert.AreEqual(Statuses.Escalated, result4.Decision);

    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskNoOne, result1.WhoToAsk);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskNoOne, result2.WhoToAsk);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskNoOne, result3.WhoToAsk);
  }

  [Test]
  public void LowCostTransitsAreRefundedWhenManyTransits()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var claim = CreateClaim(ATransit(1L), 39);
    resolver.Resolve(claim, Client.Types.Normal, 40, 15, 10);
    var claim2 = CreateClaim(ATransit(2L), 39);
    resolver.Resolve(claim2, Client.Types.Normal, 40, 15, 10);
    var claim3 = CreateClaim(ATransit(3L), 39);
    resolver.Resolve(claim3, Client.Types.Normal, 40, 15, 10);
    //and
    var claim4 = CreateClaim(ATransit(4L), 39);

    //when
    var result = resolver.Resolve(claim4, Client.Types.Normal, 40, 10, 9);

    //then
    Assert.AreEqual(Statuses.Refunded, result.Decision);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskNoOne, result.WhoToAsk);
  }

  [Test]
  public void HighCostTransitsAreEscalatedEvenWithManyTransits()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var claim = CreateClaim(ATransit(1L), 39);
    resolver.Resolve(claim, Client.Types.Normal, 40, 15, 10);
    var claim2 = CreateClaim(ATransit(2L), 39);
    resolver.Resolve(claim2, Client.Types.Normal, 40, 15, 10);
    var claim3 = CreateClaim(ATransit(3L), 39);
    resolver.Resolve(claim3, Client.Types.Normal, 40, 15, 10);
    //and
    var claim4 = CreateClaim(ATransit(4L), 50);

    //when
    var result = resolver.Resolve(claim4, Client.Types.Normal, 40, 12, 10);

    //then
    Assert.AreEqual(Statuses.Escalated, result.Decision);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskClient, result.WhoToAsk);
  }

  [Test]
  public void HighCostTransitsAreEscalatedWhenFewTransits()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var claim = CreateClaim(ATransit(1L), 39);
    resolver.Resolve(claim, Client.Types.Normal, 40, 15, 10);
    var claim2 = CreateClaim(ATransit(2L), 39);
    resolver.Resolve(claim2, Client.Types.Normal, 40, 15, 10);
    var claim3 = CreateClaim(ATransit(3L), 39);
    resolver.Resolve(claim3, Client.Types.Normal, 40, 15, 10);
    //and
    var claim4 = CreateClaim(ATransit(4L), 50);

    //when
    var result = resolver.Resolve(claim4, Client.Types.Normal, 40, 2, 10);

    //then
    Assert.AreEqual(Statuses.Escalated, result.Decision);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskDriver, result.WhoToAsk);
  }

  private Transit ATransit(long? id)
  {
    var transit = new Transit(id);
    return transit;
  }

  private Claim CreateClaim(Transit transit, int transitPrice)
  {
    var claim = new Claim
    {
      TransitId = transit.Id,
      TransitPrice = new Money(transitPrice),
      OwnerId = 1
    };
    return claim;
  }
}